using AdminDashboard.Models;
using AdminDashboard.Models.TrangThai;
using AdminDashboard.Models.ViewModels;
using AdminDashboard.Patterns;
using AdminDashboard.Patterns.Iterator;
using AdminDashboard.TransportDBContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminDashboard.Services
{
    public class BanVeService : IBanVeService
    {
        private readonly Db27524Context _context;

        public BanVeService(Db27524Context context)
        {
            _context = context;
        }

        public async Task<List<ChuyenXe>> GetChuyenXeBanVeAsync(string nhaXeId, DateTime ngayDi)
        {
            var query = _context.ChuyenXe
            .Include(c => c.LoTrinh).ThenInclude(lt => lt.TramDiNavigation)
            .Include(c => c.LoTrinh).ThenInclude(lt => lt.TramToiNavigation)
            .Include(c => c.Xe)
            .AsQueryable();

            var builder = new ChuyenXeQueryBuilder(query)
                .FilterByNhaXe(nhaXeId)
                .FilterByDate(ngayDi)
                .SortByTime();

            return await builder.Build().ToListAsync();
        }

        public async Task<SoDoGheDTO> GetSoDoGheAsync(string chuyenId, string nhaXeId)
        {
            var cx = await _context.ChuyenXe
                .Include(c => c.Xe)
                .Include(c => c.LoTrinh).ThenInclude(lt => lt.TramDiNavigation)
                .Include(c => c.LoTrinh).ThenInclude(lt => lt.TramToiNavigation)
                .FirstOrDefaultAsync(c => c.ChuyenId == chuyenId);

            if (cx == null || cx.Xe.NhaXeId != nhaXeId) return null;

            var veDaBan = await _context.Ve
               .Include(v => v.DonHang)
               .Include(v => v.Ghe)
               .Where(v => v.DonHang.ChuyenId == chuyenId && v.DonHang.TrangThaiThanhToan != "Da huy")
               .Select(v => new VeBanInfo // Dùng class DTO cụ thể hoặc anonymous type
               {
                   // QUAN TRỌNG: Đây là tên thuộc tính sẽ dùng bên View
                   SoGhe = v.Ghe.SoGhe,
                   TenKhach = v.DonHang.HoTenNguoiDat ?? "Khách Online",
                   Sdt = v.DonHang.SdtNguoiDat ?? "---",
                   TrangThai = v.DonHang.TrangThaiThanhToan
               })
               .ToListAsync();

            var tongMacDinh = cx.Xe.SoLuongGhe > 0 ? cx.Xe.SoLuongGhe : 40;

            var danhSachGhe = (cx.Xe.DanhSachGhe ?? new List<Ghe>())
                .OrderBy(g => int.TryParse(g.SoGhe, out var seatNo) ? seatNo : int.MaxValue)
                .ToList();

            // Nếu xe chưa có đủ danh sách ghế trong DB, tự bù để sơ đồ không bị thiếu ghế
            if (!danhSachGhe.Any())
            {
                danhSachGhe = Enumerable.Range(1, tongMacDinh)
                    .Select(i => new Ghe
                    {
                        GheID = string.Empty,
                        XeId = cx.XeId,
                        SoGhe = i.ToString("D2"),
                        TrangThai = "Trống"
                    })
                    .ToList();
            }
            else if (danhSachGhe.Count < tongMacDinh)
            {
                var existingSeatNumbers = danhSachGhe
                    .Select(g => g.SoGhe)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var gheConThieu = Enumerable.Range(1, tongMacDinh)
                    .Select(i => i.ToString("D2"))
                    .Where(so => !existingSeatNumbers.Contains(so))
                    .Select(so => new Ghe
                    {
                        GheID = string.Empty,
                        XeId = cx.XeId,
                        SoGhe = so,
                        TrangThai = "Trống"
                    });

                danhSachGhe.AddRange(gheConThieu);

                danhSachGhe = danhSachGhe
                    .OrderBy(g => int.TryParse(g.SoGhe, out var seatNo) ? seatNo : int.MaxValue)
                    .ToList();
            }

            var soldLookup = veDaBan
                .GroupBy(v => v.SoGhe, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var seatSource = danhSachGhe
                .Select(g =>
                {
                    soldLookup.TryGetValue(g.SoGhe, out var ve);

                    return new SeatDisplayItem
                    {
                        GheId = g.GheID,
                        SoGhe = g.SoGhe,
                        TrangThai = ve == null ? "Trong" : "DaBan",
                        TenKhach = ve?.TenKhach,
                        SoDienThoai = ve?.Sdt
                    };
                })
                .ToList();

            var seatCollection = new SeatCollection(seatSource);

            // Iterator 1: lấy toàn bộ ghế để render
            var soDoGhe = MaterializeIterator(seatCollection.CreateAllIterator());

            // Iterator 2: đếm ghế trống
            var soGheTrong = CountIterator(seatCollection.CreateAvailableIterator());

            // Iterator 3: đếm ghế đã bán
            var soGheDaBan = CountIterator(seatCollection.CreateSoldIterator());

            return new SoDoGheDTO
            {
                ChuyenXe = cx,
                VeDaBan = veDaBan,
                SoDoGhe = soDoGhe,
                TongSoGhe = seatCollection.Count,
                SoGheTrong = soGheTrong,
                SoGheDaBan = soGheDaBan
            };
        }

        public async Task<KetQuaBanVe> DatVeTaiQuayAsync(DatVeTaiQuayRequest req, string nhaXeId, string tenNhanVien)
        {
            var chuyenXe = await _context.ChuyenXe
                .Include(c => c.Xe)
                .FirstOrDefaultAsync(c => c.ChuyenId == req.ChuyenId);

            if (chuyenXe == null) return new KetQuaBanVe { Success = false, Message = "Chuyến xe không tồn tại." };

            // 1. Tách danh sách số ghế từ chuỗi (Vd: "01, 02" -> ["01", "02"])
            var danhSachSoGhe = req.SoGhe.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (danhSachSoGhe.Length > 5)
                return new KetQuaBanVe { Success = false, Message = "Không được đặt quá 5 vé một lần." };

            try
            {
                // 2. Tạo MỘT đơn hàng chung cho tất cả các ghế này
                var donHang = TicketFactory.CreateDonHang(
                    req.ChuyenId,
                    req.GiaVe, // Đây là tổng tiền đã tính ở giao diện
                    req.HoTen,
                    req.SoDienThoai,
                    req.GhiChu,
                    req.DaThanhToan,
                    tenNhanVien
                );
                _context.DonHang.Add(donHang);

                // 3. Lặp qua từng số ghế để tạo vé tương ứng
                foreach (var soGhe in danhSachSoGhe)
                {
                    var sGhe = soGhe.Trim();
                    // Tìm hoặc tạo Ghế trong DB
                    var gheDb = await _context.Ghe
                        .FirstOrDefaultAsync(g => g.XeId == chuyenXe.XeId && g.SoGhe == sGhe);

                    if (gheDb == null)
                    {
                        gheDb = new Ghe { GheID = Guid.NewGuid().ToString(), SoGhe = sGhe, XeId = chuyenXe.XeId, TrangThai = "Trong" };
                        _context.Ghe.Add(gheDb);
                        await _context.SaveChangesAsync();
                    }

                    // Kiểm tra xem ghế đã bị ai khác đặt chưa (Race condition)
                    var gheDaDat = await _context.Ve.AnyAsync(v =>
                        v.DonHang.ChuyenId == req.ChuyenId &&
                        v.GheID == gheDb.GheID &&
                        v.DonHang.TrangThaiThanhToan != "Da huy");

                    if (gheDaDat) return new KetQuaBanVe { Success = false, Message = $"Ghế {sGhe} đã có người đặt trước đó." };

                    // Tạo vé cho từng ghế
                    var giaVeMoiGhe = req.GiaVe / danhSachSoGhe.Length; // Chia đều tổng tiền cho mỗi vé
                    var ve = TicketFactory.CreateVe(donHang.DonHangId, gheDb.GheID, giaVeMoiGhe);
                    _context.Ve.Add(ve);
                }

                await _context.SaveChangesAsync();
                return new KetQuaBanVe { Success = true, Message = $"Xuất thành công {danhSachSoGhe.Length} vé!" };
            }
            catch (Exception ex)
            {
                return new KetQuaBanVe { Success = false, Message = "Lỗi hệ thống: " + ex.Message };
            }
        }

        // --- HÀM HỦY VÉ (ĐÃ SỬA LẠI LOGIC TÌM GHẾ) ---
        public async Task<KetQuaBanVe> HuyVeAsync(string chuyenId, string soGhe, string nhaXeId)
        {
            var ve = await _context.Ve
                .Include(v => v.DonHang).ThenInclude(dh => dh.ChuyenXe.Xe)
                .Include(v => v.Ghe) // Include bảng Ghe để so sánh số ghế
                .FirstOrDefaultAsync(v => v.DonHang.ChuyenId == chuyenId
                                      && v.Ghe.SoGhe == soGhe // Sửa: So sánh chuỗi "A01" với cột SoGhe
                                      && v.DonHang.TrangThaiThanhToan != "Da huy");

            if (ve == null) return new KetQuaBanVe { Success = false, Message = "Không tìm thấy vé này hoặc vé đã hủy." };
            if (ve.DonHang.ChuyenXe.Xe.NhaXeId != nhaXeId) return new KetQuaBanVe { Success = false, Message = "Bạn không có quyền hủy vé của nhà xe khác." };

            ve.DonHang.TrangThaiThanhToan = "Da huy";
            await _context.SaveChangesAsync();

            return new KetQuaBanVe { Success = true, Message = "Đã hủy vé thành công." };
        }
        // --- TÌM KIẾM VÉ ---
        public async Task<VeBanInfo> TraCuuVeAsync(string keyword, string nhaXeId)
        {
            // Tìm trong DB (Join bảng Ve, DonHang, Ghe, ChuyenXe)
            var ve = await _context.Ve
                .Include(v => v.Ghe)
                .Include(v => v.DonHang).ThenInclude(dh => dh.ChuyenXe.LoTrinh.TramDiNavigation)
                .Include(v => v.DonHang).ThenInclude(dh => dh.ChuyenXe.LoTrinh.TramToiNavigation)
                .Include(v => v.DonHang).ThenInclude(dh => dh.ChuyenXe.Xe)
                .Where(v => v.DonHang.ChuyenXe.Xe.NhaXeId == nhaXeId) // Chỉ tìm vé của nhà xe mình
                .FirstOrDefaultAsync(v => v.VeId == keyword
                                       || v.DonHangId == keyword
                                       || v.DonHang.SdtNguoiDat == keyword); // Tìm theo 3 tiêu chí

            if (ve == null) return null;

            return new VeBanInfo
            {
                GheID = ve.VeId, // Tạm dùng field này để trả về VeId cho tiện
                SoGhe = ve.Ghe.SoGhe,
                TenKhach = ve.DonHang.HoTenNguoiDat,
                Sdt = ve.DonHang.SdtNguoiDat,
                TrangThai = ve.DonHang.TrangThaiThanhToan,
                ChuyenId = ve.DonHang.ChuyenId,
                // Lợi dụng các field string thừa để trả về thông tin lộ trình (DTO hơi "lười" tí nhưng tiện)
                // Trong dự án thật nên tạo DTO riêng cho chi tiết
            };
        }

        // --- ĐỔI GHẾ (Đã hỗ trợ đổi sang chuyến khác) ---
        public async Task<KetQuaBanVe> DoiGheAsync(string veId, string soGheMoi, string chuyenIdMoi, string nhaXeId)
        {
            // 1. Lấy thông tin vé cũ
            var veCu = await _context.Ve
                .Include(v => v.DonHang).ThenInclude(dh => dh.ChuyenXe)
                .FirstOrDefaultAsync(v => v.VeId == veId);

            if (veCu == null) return new KetQuaBanVe { Success = false, Message = "Không tìm thấy vé." };

            // 2. Xác định chuyến xe khách muốn đổi sang (Nếu không chọn chuyến mới thì giữ chuyến cũ)
            string targetChuyenId = string.IsNullOrEmpty(chuyenIdMoi) ? veCu.DonHang.ChuyenId : chuyenIdMoi;

            var chuyenMoi = await _context.ChuyenXe
                .FirstOrDefaultAsync(c => c.ChuyenId == targetChuyenId);

            if (chuyenMoi == null) return new KetQuaBanVe { Success = false, Message = "Chuyến xe không tồn tại." };

            // 3. Kiểm tra ghế mới trên chuyến xe mục tiêu
            var gheMoi = await _context.Ghe
                .FirstOrDefaultAsync(g => g.XeId == chuyenMoi.XeId && g.SoGhe == soGheMoi);

            if (gheMoi == null) return new KetQuaBanVe { Success = false, Message = $"Ghế {soGheMoi} không tồn tại trên xe này." };

            // 4. Check xem ghế mới đã có ai mua ở chuyến xe mục tiêu chưa
            bool daCoNguoiDat = await _context.Ve.AnyAsync(v =>
                v.DonHang.ChuyenId == targetChuyenId &&
                v.GheID == gheMoi.GheID &&
                v.DonHang.TrangThaiThanhToan != "Da huy");

            if (daCoNguoiDat) return new KetQuaBanVe { Success = false, Message = $"Ghế {soGheMoi} đã có người khác đặt rồi." };

            // 5. Cập nhật vé sang ghế mới
            veCu.GheID = gheMoi.GheID;

            // 6. Nếu là đổi sang chuyến khác -> Cập nhật ChuyenId của Đơn Hàng
            if (veCu.DonHang.ChuyenId != targetChuyenId)
            {
                veCu.DonHang.ChuyenId = targetChuyenId;
            }

            await _context.SaveChangesAsync();
            return new KetQuaBanVe { Success = true, Message = $"Đổi vé thành công!" };
        }

        private static List<SeatDisplayItem> MaterializeIterator(ISeatIterator iterator)
        {
            var result = new List<SeatDisplayItem>();
            iterator.Reset();

            while (iterator.HasNext())
            {
                result.Add(iterator.Next());
            }

            return result;
        }

        private static int CountIterator(ISeatIterator iterator)
        {
            var count = 0;
            iterator.Reset();

            while (iterator.HasNext())
            {
                iterator.Next();
                count++;
            }

            return count;
        }
    }
}