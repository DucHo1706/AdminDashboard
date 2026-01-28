using AdminDashboard.Models;
using AdminDashboard.Models.TrangThai;
using AdminDashboard.Models.ViewModels;
using AdminDashboard.TransportDBContext;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminDashboard.Services
{
    public class ChuyenXeService : IChuyenXeService
    {
        private readonly Db27524Context _context;
        private readonly IImageService _imageService;

        public ChuyenXeService(Db27524Context context, IImageService imageService)
        {
            _context = context;
            _imageService = imageService;
        }

        // 1. TẠO LỊCH TỰ ĐỘNG
        // 1. TẠO LỊCH TỰ ĐỘNG (ĐÃ FIX LỖI CHỈ TẠO 1 CHUYẾN/NGÀY)
        public async Task<KetQuaTaoLich> TaoLichTuDongAsync(TaoLichChayRequest req, string nhaXeId)
        {
            var isMyCar = await _context.Xe.AnyAsync(x => x.XeId == req.XeId && x.NhaXeId == nhaXeId);
            if (!isMyCar) return new KetQuaTaoLich { Success = 0, Skipped = 0, Message = "Xe không hợp lệ." };

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 1. Upload ảnh (nếu có)
                    List<string> uploadedUrls = new List<string>();
                    if (req.Images?.Count > 0) uploadedUrls = await _imageService.UploadImagesAsync(req.Images);

                    int countSuccess = 0;
                    int countSkipped = 0;

                    // 2. Lấy tất cả chuyến xe đã có trong khoảng thời gian này để check trùng
                    var existingTrips = await _context.ChuyenXe
                        .Where(c => c.XeId == req.XeId && c.TrangThai != TrangThaiChuyenXe.DaHuy && c.NgayDi >= req.TuNgay && c.NgayDi <= req.DenNgay)
                        .Select(c => new { c.NgayDi, c.GioDi, c.GioDenDuKien }).ToListAsync(); // Bỏ check LoTrinhId để xe không bị trùng giờ chạy dù khác tuyến

                    // Danh sách các khe thời gian bận
                    var busySlots = existingTrips.Select(x => new
                    {
                        Start = x.NgayDi.Date.Add(x.GioDi),
                        End = x.NgayDi.Date.Add(x.GioDenDuKien).AddDays(x.GioDenDuKien < x.GioDi ? 1 : 0)
                    }).ToList();

                    var currentDate = req.TuNgay;

                    // --- VÒNG LẶP NGÀY ---
                    while (currentDate <= req.DenNgay)
                    {
                        // BỎ đoạn check "daChayTuyenNayHomNay" để cho phép 1 ngày chạy nhiều chuyến

                        var currentTime = req.KhungGioTu;

                        // --- VÒNG LẶP GIỜ (TẠO NHIỀU CHUYẾN TRONG NGÀY) ---
                        while (currentTime <= req.KhungGioDen)
                        {
                            // Tính thời gian bắt đầu và kết thúc của chuyến dự kiến
                            DateTime newTripStart = currentDate.Date.Add(currentTime);
                            DateTime newTripEnd = newTripStart.Add(req.ThoiGianDiChuyen);
                            TimeSpan gioDenDB = newTripEnd.TimeOfDay;

                            // Kiểm tra xem khung giờ này xe có đang bận chạy chuyến khác không
                            bool isConflict = busySlots.Any(slot => newTripStart < slot.End && newTripEnd > slot.Start);

                            if (!isConflict)
                            {
                                // Tạo chuyến mới
                                var chuyenXe = new ChuyenXe
                                {
                                    ChuyenId = Guid.NewGuid().ToString("N")[..8],
                                    LoTrinhId = req.LoTrinhId,
                                    XeId = req.XeId,
                                    NgayDi = currentDate,
                                    GioDi = currentTime,
                                    GioDenDuKien = gioDenDB,
                                    TrangThai = TrangThaiChuyenXe.ChoDuyet
                                };
                                _context.ChuyenXe.Add(chuyenXe);

                                // Gán ảnh
                                foreach (var url in uploadedUrls)
                                    _context.ChuyenXeImage.Add(new ChuyenXeImage { ChuyenId = chuyenXe.ChuyenId, ImageUrl = url });

                                // Cập nhật lại danh sách bận để chuyến sau trong ngày không bị trùng chuyến vừa tạo
                                busySlots.Add(new { Start = newTripStart, End = newTripEnd });
                                countSuccess++;
                            }
                            else
                            {
                                countSkipped++;
                            }

                            // QUAN TRỌNG: Luôn tăng thời gian để qua chuyến tiếp theo (VD: 7h -> 9h -> 11h...)
                            currentTime = currentTime.Add(TimeSpan.FromMinutes(req.GianCachPhut));
                        }

                        currentDate = currentDate.AddDays(1);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new KetQuaTaoLich { Success = countSuccess, Skipped = countSkipped, Message = "Success" };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    // Log lỗi ra để biết chi tiết nếu cần
                    throw new Exception("Lỗi tạo lịch: " + ex.Message);
                }
            });
        }

        // 2. CẬP NHẬT CHUYẾN XE (Logic chuyển từ Controller sang)
        public async Task<string> UpdateChuyenXeAsync(ChuyenXe model, string deletedImages, IFormFileCollection newImages, string nhaXeId)
        {
            // 1. Lấy dữ liệu gốc từ DB
            var cx = await _context.ChuyenXe
                .Include(c => c.Images)
                .Include(c => c.Xe)
                .FirstOrDefaultAsync(c => c.ChuyenId == model.ChuyenId);

            if (cx == null) return "Không tìm thấy chuyến xe.";
            if (cx.Xe.NhaXeId != nhaXeId) return "Bạn không có quyền sửa chuyến xe này.";

            // 2. Xác định xem người dùng có đang sửa "Thông tin quan trọng" không?
            // (Thông tin quan trọng = Lộ trình, Ngày đi, Giờ chạy)
            bool isCriticalChange = (cx.GioDi != model.GioDi)
                                 || (cx.LoTrinhId != model.LoTrinhId)
                                 || (cx.NgayDi != model.NgayDi)
                                 || (cx.GioDenDuKien != model.GioDenDuKien);

            string resultMessage = "Success";

            // --- LOGIC PHÂN QUYỀN THEO TRẠNG THÁI ---

            // TRƯỜNG HỢP 1: Chờ Duyệt (Thoải mái sửa)
            if (cx.TrangThai == TrangThaiChuyenXe.ChoDuyet)
            {
                // Không cần làm gì thêm, cho phép update bên dưới
            }
            // TRƯỜNG HỢP 2: Đã Lên Lịch (Đã duyệt nhưng chưa bán)
            else if (cx.TrangThai == TrangThaiChuyenXe.DaLenLich)
            {
                // Nếu sửa thông tin quan trọng -> Bắt Admin duyệt lại
                if (isCriticalChange)
                {
                    cx.TrangThai = TrangThaiChuyenXe.ChoDuyet; // Reset trạng thái về -1
                    resultMessage = "Warning:Reapproval"; // Mã báo Controller biết để hiện cảnh báo vàng
                }
                // Nếu chỉ đổi Xe/Tài xế/Ảnh -> Giữ nguyên trạng thái "Đã Lên Lịch"
            }
            // TRƯỜNG HỢP 3: Đang Mở Bán / Đang Chạy / Hoàn Thành...
            else
            {
                // CẤM TUYỆT ĐỐI sửa thông tin quan trọng (vì khách đã đặt vé rồi)
                if (isCriticalChange)
                {
                    return "CẤM: Không thể thay đổi Lịch trình/Ngày/Giờ khi chuyến xe đang mở bán hoặc đã hoạt động. Chỉ được phép đổi Xe hoặc Tài xế.";
                }

                // Nếu không sửa thông tin quan trọng (tức là chỉ đổi Xe do hư hỏng, đổi Tài xế...) -> CHO PHÉP
            }

            // 3. Tiến hành cập nhật dữ liệu vào DB
            cx.LoTrinhId = model.LoTrinhId;
            cx.XeId = model.XeId;
            cx.NgayDi = model.NgayDi;
            cx.GioDi = model.GioDi;
            cx.GioDenDuKien = model.GioDenDuKien;

            // Lưu ý: Nếu form Edit của bạn có cho chọn Tài xế, hãy bỏ comment dòng dưới:
            // cx.TaiXeId = model.TaiXeId; 

            // --- XỬ LÝ ẢNH (Giữ nguyên logic cũ) ---
            // 1. Upload ảnh mới
            if (newImages?.Count > 0)
            {
                var urls = await _imageService.UploadImagesAsync(newImages);
                foreach (var url in urls)
                {
                    _context.ChuyenXeImage.Add(new ChuyenXeImage { ChuyenId = cx.ChuyenId, ImageUrl = url });
                }
            }

            // 2. Xóa ảnh cũ
            if (!string.IsNullOrEmpty(deletedImages))
            {
                var ids = deletedImages.Split(',').Select(int.Parse).ToList();
                var imgsToRemove = cx.Images.Where(i => ids.Contains(i.ImageId)).ToList();
                _context.ChuyenXeImage.RemoveRange(imgsToRemove);
            }

            await _context.SaveChangesAsync();

            return resultMessage;
        }

        // 3. XÓA CHUYẾN XE
        public async Task<string> DeleteChuyenXeAsync(string id, string nhaXeId)
        {
            var cx = await _context.ChuyenXe.Include(c => c.Xe).FirstOrDefaultAsync(c => c.ChuyenId == id);

            if (cx == null) return "Không tìm thấy chuyến xe.";
            if (cx.Xe.NhaXeId != nhaXeId) return "Bạn không có quyền xóa.";

            // Chỉ cho xóa khi Chờ Duyệt hoặc Đã Lên Lịch (chưa bán vé)
            if (cx.TrangThai == TrangThaiChuyenXe.DaHoanThanh || cx.TrangThai == TrangThaiChuyenXe.DangMoBanVe || cx.TrangThai == TrangThaiChuyenXe.DangDiChuyen)
            {
                return "Không thể xóa chuyến xe đang hoạt động hoặc đã hoàn thành.";
            }

            _context.ChuyenXe.Remove(cx);
            await _context.SaveChangesAsync();
            return "Success";
        }
        public async Task<int> DuyetNhieuChuyenAsync(List<string> ids, string adminId)
        {
            // Lấy danh sách các chuyến đang Chờ Duyệt có ID nằm trong danh sách gửi lên
            var chuyenXes = await _context.ChuyenXe
                .Where(c => ids.Contains(c.ChuyenId) && c.TrangThai == TrangThaiChuyenXe.ChoDuyet)
                .ToListAsync();

            if (!chuyenXes.Any()) return 0;

            foreach (var cx in chuyenXes)
            {
                cx.TrangThai = TrangThaiChuyenXe.DaLenLich; // Chuyển sang Đã Lên Lịch
                                                            // cx.NguoiDuyetId = adminId; (Nếu có trường này thì gán vào)
            }

            await _context.SaveChangesAsync();
            return chuyenXes.Count; // Trả về số lượng đã duyệt thành công
        }
        // --- 5. PHÂN CÔNG TÀI XẾ (Code đã sửa lỗi & thêm check trạng thái) ---
        public async Task<string> PhanCongTaiXeAsync(string chuyenId, string taiXeId, string nhaXeId)
        {
            var cx = await _context.ChuyenXe
                .Include(c => c.Xe)
                .FirstOrDefaultAsync(c => c.ChuyenId == chuyenId);

            if (cx == null) return "Không tìm thấy chuyến xe.";
            if (cx.Xe.NhaXeId != nhaXeId) return "Bạn không có quyền thao tác.";

            // --- SỬA 1: CHECK TRẠNG THÁI CHUYẾN XE ---
            // Chỉ chặn khi chuyến xe đã "chốt sổ" (Đang chạy, Đã xong, Đã hủy)
            // Các trạng thái còn lại (Chờ duyệt, Đã lên lịch, Đang bán vé...) đều cho phép đổi tài xế thoải mái.
            if (cx.TrangThai == TrangThaiChuyenXe.DangDiChuyen ||
                cx.TrangThai == TrangThaiChuyenXe.DaHoanThanh ||
                cx.TrangThai == TrangThaiChuyenXe.DaHuy)
            {
                return $"Không thể thay đổi tài xế khi chuyến xe đang ở trạng thái '{cx.TrangThai}'.";
            }

            // Nếu người dùng chọn "Chưa phân công" (Gỡ tài xế) -> Cho phép luôn
            if (string.IsNullOrEmpty(taiXeId))
            {
                cx.TaiXeId = null;
                await _context.SaveChangesAsync();
                return "Success";
            }

            // --- SỬA 2: FIX LỖI LINQ (InvalidOperationException) ---
            // Nguyên nhân lỗi cũ: EF Core không dịch được đoạn code (NgayDi + GioDi).AddHours() sang SQL.
            // Giải pháp: Tải dữ liệu thô về RAM rồi mới tính toán.

            // B1: Tính thời gian của chuyến xe HIỆN TẠI (Đang muốn gán)
            double thoiGianChay = 5; // Giả sử 5 tiếng (Bạn nên lấy từ DB nếu có cột ThoiGianDiChuyen)
            DateTime startNew = cx.NgayDi.Date.Add(cx.GioDi);
            DateTime endNew = startNew.AddHours(thoiGianChay);

            // B2: Lọc sơ bộ từ DB (Chỉ lấy các chuyến của tài xế này trong khoảng +/- 1 ngày)
            // Bước này giúp giảm tải bộ nhớ, chỉ lấy vài dòng cần thiết về
            var potentialTrips = await _context.ChuyenXe
                .Where(c => c.TaiXeId == taiXeId
                         && c.ChuyenId != chuyenId // Không so với chính nó
                         && c.TrangThai != TrangThaiChuyenXe.DaHuy // Không tính chuyến hủy
                         && c.NgayDi >= startNew.Date.AddDays(-1)
                         && c.NgayDi <= startNew.Date.AddDays(1))
                .ToListAsync(); // <--- QUAN TRỌNG: Tải về RAM tại đây

            // B3: Tính toán check trùng trong RAM (C# xử lý được TimeSpan/AddHours thoải mái)
            bool isBusy = potentialTrips.Any(c =>
            {
                var startOld = c.NgayDi.Date.Add(c.GioDi);
                var endOld = startOld.AddHours(thoiGianChay);

                // Công thức kiểm tra 2 khoảng thời gian có giao nhau không:
                // (StartA < EndB) && (EndA > StartB)
                return startNew < endOld && endNew > startOld;
            });

            if (isBusy) return "Tài xế này bị trùng lịch chạy với chuyến khác. Vui lòng chọn người khác.";

            // Nếu mọi thứ ổn -> Lưu vào DB
            cx.TaiXeId = taiXeId;
            await _context.SaveChangesAsync();
            return "Success";
        }
    }
}