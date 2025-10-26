using AdminDashboard.Models;
using AdminDashboard.Models.TrangThai;
using AdminDashboard.TransportDBContext;
using AdminDashboard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AdminDashboard.Controllers
{
    [Authorize(Roles = "Admin,TaiXe")]
    public class TaiXeController : Controller
    {
        private readonly Db27524Context _context;
        private readonly ILogger<TaiXeController> _logger;

        public TaiXeController(Db27524Context context, ILogger<TaiXeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var admin = await _context.NguoiDung
               .FirstOrDefaultAsync(u => u.UserId == adminUserId);

            if (admin == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin admin. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Account");
            }

            var taiXes = await _context.TaiXe
                .Include(t => t.NguoiDung)
                .Include(t => t.Admin)
          .Where(t => t.AdminId == admin.UserId)

                .ToListAsync();

            return View(taiXes);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaiXeRegistrationViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var admin = await _context.NguoiDung.FirstOrDefaultAsync(u => u.UserId == adminUserId);

                if (admin == null)
                {
                    ModelState.AddModelError("", "Không tìm thấy thông tin admin. Vui lòng đăng nhập lại.");
                    return View(model);
                }

                // Kiểm tra email đã tồn tại trong NguoiDung
                if (await _context.NguoiDung.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email đã tồn tại.");
                    return View(model);
                }

                // Tạo UserId mới
                var userId = GenerateUserId();

                // Tạo NguoiDung
                var nguoiDung = new NguoiDung
                {
                    UserId = userId,
                    Email = model.Email.Trim(),
                    MatKhau = BCrypt.Net.BCrypt.HashPassword(model.MatKhau),
                    HoTen = model.HoTen.Trim(),
                    SoDienThoai = model.SoDienThoai?.Trim(),
                    NgaySinh = model.NgaySinh,
                    TrangThai = TrangThaiNguoiDung.HoatDong
                };

                _context.NguoiDung.Add(nguoiDung);
                await _context.SaveChangesAsync();

                // Gán vai trò TaiXe
                var roleId = "29cf77eb-dbda-4bf4-be3e-131265a2dc37";
                var taiXeRole = await _context.VaiTro.FirstOrDefaultAsync(r => r.RoleId == roleId);
                if (taiXeRole == null)
                {
                    taiXeRole = new VaiTro
                    {
                        RoleId = roleId,
                        TenVaiTro = "TaiXe"
                    };
                    _context.VaiTro.Add(taiXeRole);
                    await _context.SaveChangesAsync();
                }

                var userRole = new UserRole
                {
                    UserId = userId,
                    RoleId = roleId
                };
                _context.UserRole.Add(userRole);

                // Tạo TaiXe
                var taiXe = new TaiXe
                {
                    UserId = userId,
                    AdminId = admin.UserId,
                    BangLaiXe = model.BangLaiXe.Trim(),
                    NgayVaoLam = model.NgayVaoLam ?? DateTime.Now,
                    HoTen = model.HoTen.Trim(),
                    TrangThai = "Hoạt động"
                };

                _context.TaiXe.Add(taiXe);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đăng ký tài xế thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đăng ký tài xế");
                ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}. Vui lòng thử lại.");
                return View(model);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Không tìm thấy tài xế.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Kiểm tra xem tài xế có đang được phân công trong chuyến xe nào không
                var hasAssignedTrips = await _context.ChuyenXe
                    .AnyAsync(c => c.TaiXeId == userId &&
                                  (c.TrangThai == TrangThaiChuyenXe.DangDiChuyen ||
                                   c.TrangThai == TrangThaiChuyenXe.ChoKhoiHanh));

                if (hasAssignedTrips)
                {
                    TempData["ErrorMessage"] = "Không thể xóa tài xế đang có chuyến xe được phân công. Vui lòng hủy phân công trước.";
                    return RedirectToAction(nameof(Index));
                }

                // Tìm tài xế
                var taiXe = await _context.TaiXe
                    .FirstOrDefaultAsync(t => t.UserId == userId);

                if (taiXe == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy tài xế.";
                    return RedirectToAction(nameof(Index));
                }

                // Xóa phân công trong các chuyến xe (set null cho TaiXeId)
                var assignedTrips = await _context.ChuyenXe
                    .Where(c => c.TaiXeId == userId)
                    .ToListAsync();

                foreach (var trip in assignedTrips)
                {
                    trip.TaiXeId = null;
                    if (trip.TrangThai == TrangThaiChuyenXe.ChoKhoiHanh)
                    {
                        trip.TrangThai = TrangThaiChuyenXe.DaLenLich;
                    }
                }

                // Xóa tài xế
                _context.TaiXe.Remove(taiXe);

                // Xóa vai trò trong UserRole
                var userRoles = await _context.UserRole
                    .Where(ur => ur.UserId == userId)
                    .ToListAsync();
                _context.UserRole.RemoveRange(userRoles);

                // Xóa người dùng
                var nguoiDung = await _context.NguoiDung
                    .FirstOrDefaultAsync(u => u.UserId == userId);
                if (nguoiDung != null)
                {
                    _context.NguoiDung.Remove(nguoiDung);
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Xóa tài xế thành công!";
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Lỗi database khi xóa tài xế");
                TempData["ErrorMessage"] = "Lỗi database khi xóa tài xế. Có thể tài xế đang được tham chiếu ở bảng khác.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa tài xế");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa tài xế. Vui lòng thử lại.";
            }

            return RedirectToAction(nameof(Index));
        }
        private string GenerateUserId()
        {
            return Guid.NewGuid().ToString("N");
        }

        // GET: /TaiXe/LichLamViec
        //public async Task<IActionResult> LichLamViec()
        //{
        //    // Lấy ID của tài xế đang đăng nhập
        //    var idTaiXe = User.FindFirstValue(ClaimTypes.NameIdentifier);

        //    //if (string.IsNullOrEmpty(idTaiXe))
        //    //{
        //    //    return Unauthorized(); // Không tìm thấy thông tin đăng nhập
        //    //}

        //    // Lấy danh sách các chuyến xe được phân công cho tài xế này
        //    // Chỉ lấy các chuyến từ hôm nay trở về sau
        //    var lichCuaToi = await _context.ChuyenXe
        //        .Where(c => c.TaiXeId == idTaiXe && c.NgayDi >= DateTime.Today)
        //        .Include(c => c.LoTrinh.TramDiNavigation)
        //        .Include(c => c.LoTrinh.TramToiNavigation)
        //        .Include(c => c.Xe)
        //        .OrderBy(c => c.NgayDi).ThenBy(c => c.GioDi) // Sắp xếp theo thời gian
        //        .ToListAsync();

        //    return View(lichCuaToi);
        //}


        [Authorize(Roles = "Admin,TaiXe")]
        // GET: /TaiXe/LichLamViec?taiXeId=...
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> LichLamViec(string taiXeId, DateTime? tuNgay, DateTime? denNgay)
        {
            if (string.IsNullOrEmpty(taiXeId))
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin tài xế.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var admin = await _context.NguoiDung.FirstOrDefaultAsync(u => u.UserId == adminUserId);

                if (admin == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin admin.";
                    return RedirectToAction(nameof(Index));
                }

                // Kiểm tra tài xế có thuộc quản lý của admin không
                var taiXe = await _context.TaiXe
                    .Include(t => t.NguoiDung)
                    .FirstOrDefaultAsync(t => t.UserId == taiXeId && t.AdminId == admin.UserId);

                if (taiXe == null)
                {
                    TempData["ErrorMessage"] = "Tài xế không thuộc quản lý của bạn.";
                    return RedirectToAction(nameof(Index));
                }

                var startDate = tuNgay ?? DateTime.Today;
                var endDate = denNgay ?? DateTime.Today.AddDays(7);

                // Lấy lịch làm việc của tài xế cụ thể
                var lichLamViec = await _context.ChuyenXe
                    .Where(c => c.TaiXeId == taiXeId &&
                               c.NgayDi >= startDate &&
                               c.NgayDi <= endDate)
                    .Include(c => c.LoTrinh.TramDiNavigation)
                    .Include(c => c.LoTrinh.TramToiNavigation)
                    .Include(c => c.Xe)
                    .OrderBy(c => c.NgayDi)
                    .ThenBy(c => c.GioDi)
                    .ToListAsync();

                ViewBag.TaiXe = taiXe;
                ViewBag.TuNgay = startDate.ToString("yyyy-MM-dd");
                ViewBag.DenNgay = endDate.ToString("yyyy-MM-dd");

                return View(lichLamViec);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải lịch làm việc của tài xế");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải dữ liệu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /TaiXe/DanhSachChuyen?taiXeId=...
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DanhSachChuyen(string taiXeId)
        {
            if (string.IsNullOrEmpty(taiXeId))
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin tài xế.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var admin = await _context.NguoiDung.FirstOrDefaultAsync(u => u.UserId == adminUserId);

                if (admin == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin admin.";
                    return RedirectToAction(nameof(Index));
                }

                // Kiểm tra tài xế có thuộc quản lý của admin không
                var taiXe = await _context.TaiXe
                    .Include(t => t.NguoiDung)
                    .FirstOrDefaultAsync(t => t.UserId == taiXeId && t.AdminId == admin.UserId);

                if (taiXe == null)
                {
                    TempData["ErrorMessage"] = "Tài xế không thuộc quản lý của bạn.";
                    return RedirectToAction(nameof(Index));
                }

                var homNay = DateTime.Today;
                int diff = (7 + (int)homNay.DayOfWeek - (int)DayOfWeek.Monday) % 7;
                var ngayDauTuan = homNay.AddDays(-1 * diff).Date;
                var ngayCuoiTuan = ngayDauTuan.AddDays(6).Date;

                // Lấy danh sách chuyến của tài xế cụ thể
                var danhSachChuyen = await _context.ChuyenXe
                    .Include(c => c.LoTrinh).ThenInclude(lt => lt.TramDiNavigation)
                    .Include(c => c.LoTrinh).ThenInclude(lt => lt.TramToiNavigation)
                    .Include(c => c.Xe)
                    .Where(c => c.TaiXeId == taiXeId &&
                               c.NgayDi.Date >= ngayDauTuan &&
                               c.NgayDi.Date <= ngayCuoiTuan)
                    .OrderBy(c => c.NgayDi)
                    .ThenBy(c => c.GioDi)
                    .ToListAsync();

                ViewBag.TaiXe = taiXe;
                ViewBag.TuanHienTai = $"Tuần từ {ngayDauTuan:dd/MM} đến {ngayCuoiTuan:dd/MM/yyyy}";

                return View(danhSachChuyen);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách chuyến của tài xế");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải dữ liệu.";
                return RedirectToAction(nameof(Index));
            }
        }





        //=========================================================================================

        public async Task<IActionResult> PhanCongLich()
        {
            var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var admin = await _context.NguoiDung.FirstOrDefaultAsync(u => u.UserId == adminUserId);

            if (admin == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin admin. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Account");
            }

            var chuyenXeChuaPhanCong = await _context.ChuyenXe
               .Where(c => c.TaiXeId == null &&
                          c.NgayDi >= DateTime.Today &&
                          c.TrangThai != TrangThaiChuyenXe.DaHoanThanh &&
                          c.TrangThai != TrangThaiChuyenXe.DaHuy &&
                          c.TrangThai != TrangThaiChuyenXe.DangDiChuyen)
               .Include(c => c.LoTrinh)
                   .ThenInclude(lt => lt.TramDiNavigation)
               .Include(c => c.LoTrinh)
                   .ThenInclude(lt => lt.TramToiNavigation)
               .Include(c => c.Xe)
               .OrderBy(c => c.NgayDi)
               .ThenBy(c => c.GioDi)
               .ToListAsync();

            // Lấy danh sách tài xế thuộc quản lý của admin
            var taiXes = await _context.TaiXe
                .Where(t => t.AdminId == admin.UserId && t.TrangThai == "Hoạt động")
                .Include(t => t.NguoiDung)
                .ToListAsync();

            ViewBag.TaiXes = new SelectList(taiXes, "UserId", "HoTen");
            return View(chuyenXeChuaPhanCong);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PhanCongLich(string chuyenId, string taiXeId)
        {
            _logger.LogInformation($"Bắt đầu phân công: chuyenId={chuyenId}, taiXeId={taiXeId}");

            if (string.IsNullOrEmpty(chuyenId) || string.IsNullOrEmpty(taiXeId))
            {
                TempData["ErrorMessage"] = "Thông tin phân công không hợp lệ.";
                _logger.LogWarning("Thông tin phân công không hợp lệ: chuyenId hoặc taiXeId null/empty");
                return RedirectToAction(nameof(PhanCongLich));
            }

            try
            {
                var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var admin = await _context.NguoiDung.FirstOrDefaultAsync(u => u.UserId == adminUserId);

                if (admin == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin admin.";
                    _logger.LogWarning("Không tìm thấy admin với UserId: {adminUserId}", adminUserId);
                    return RedirectToAction(nameof(PhanCongLich));
                }

                _logger.LogInformation($"Admin tìm thấy: {admin.UserId}");

                var chuyenXe = await _context.ChuyenXe
                   .FirstOrDefaultAsync(c => c.ChuyenId == chuyenId && c.TaiXeId == null);

                if (chuyenXe == null)
                {
                    var chuyenXeDaCoTaiXe = await _context.ChuyenXe
                       .FirstOrDefaultAsync(c => c.ChuyenId == chuyenId && c.TaiXeId != null);

                    if (chuyenXeDaCoTaiXe != null)
                    {
                        TempData["ErrorMessage"] = "Chuyến xe đã được phân công tài xế trước đó.";
                        _logger.LogWarning("Chuyến xe {chuyenId} đã có tài xế: {taiXeId}", chuyenId, chuyenXeDaCoTaiXe.TaiXeId);
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Chuyến xe không tồn tại.";
                        _logger.LogWarning("Không tìm thấy chuyến xe với ID: {chuyenId}", chuyenId);
                    }
                    return RedirectToAction(nameof(PhanCongLich));
                }

                _logger.LogInformation($"Chuyến xe tìm thấy: {chuyenXe.ChuyenId}, Trạng thái: {chuyenXe.TrangThai}");

                var taiXe = await _context.TaiXe
                   .FirstOrDefaultAsync(t => t.UserId == taiXeId && t.AdminId == admin.UserId && t.TrangThai == "Hoạt động");

                if (taiXe == null)
                {
                    var allTaiXe = await _context.TaiXe
                       .Where(t => t.AdminId == admin.UserId)
                       .ToListAsync();

                    _logger.LogWarning($"Tài xế không tồn tại hoặc không thuộc quản lý. Tài xế tìm kiếm: {taiXeId}, Admin: {admin.UserId}");
                    _logger.LogWarning($"Danh sách tài xế thuộc admin {admin.UserId}: {string.Join(", ", allTaiXe.Select(t => t.UserId))}");

                    TempData["ErrorMessage"] = "Tài xế không tồn tại hoặc không thuộc quản lý của bạn.";
                    return RedirectToAction(nameof(PhanCongLich));
                }

                _logger.LogInformation($"Tài xế tìm thấy: {taiXe.UserId}, Tên: {taiXe.HoTen}");

                var isTrungLich = await KiemTraTrungLich(taiXeId, chuyenXe.NgayDi, chuyenXe.GioDi, chuyenXe.GioDenDuKien);
                if (isTrungLich)
                {
                    TempData["ErrorMessage"] = "Tài xế đã có lịch chạy trùng với thời gian này. Vui lòng chọn tài xế khác.";
                    _logger.LogWarning("Tài xế {taiXeId} bị trùng lịch với chuyến {chuyenId}", taiXeId, chuyenId);
                    return RedirectToAction(nameof(PhanCongLich));
                }

                _logger.LogInformation("Không có trùng lịch, tiến hành phân công...");

                chuyenXe.TaiXeId = taiXeId;

                if (chuyenXe.TrangThai == TrangThaiChuyenXe.DaLenLich || chuyenXe.TrangThai == TrangThaiChuyenXe.DangMoBanVe)
                {
                    chuyenXe.TrangThai = TrangThaiChuyenXe.ChoKhoiHanh;
                    _logger.LogInformation($"Cập nhật trạng thái chuyến xe từ {chuyenXe.TrangThai} sang ChoKhoiHanh");
                }

                _context.ChuyenXe.Update(chuyenXe);
                var result = await _context.SaveChangesAsync();

                _logger.LogInformation($"SaveChanges trả về: {result} row(s) affected");
                _logger.LogInformation($"Admin {admin.UserId} đã phân công tài xế {taiXeId} cho chuyến xe {chuyenId}");

                TempData["SuccessMessage"] = "Phân công lịch thành công!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi phân công lịch cho tài xế");
                TempData["ErrorMessage"] = $"Có lỗi xảy ra khi phân công: {ex.Message}. Vui lòng thử lại.";
            }

            return RedirectToAction(nameof(PhanCongLich));
        }


        public async Task<IActionResult> LichSuPhanCong()
        {
            var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var admin = await _context.NguoiDung.FirstOrDefaultAsync(u => u.UserId == adminUserId);

            if (admin == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin admin.";
                return RedirectToAction("Login", "Account");
            }

            var chuyenXeDaPhanCong = await _context.ChuyenXe
                .Where(c => c.TaiXeId != null &&
                           c.NgayDi >= DateTime.Today.AddMonths(-1))
                .Include(c => c.LoTrinh)
                    .ThenInclude(lt => lt.TramDiNavigation)
                .Include(c => c.LoTrinh)
                    .ThenInclude(lt => lt.TramToiNavigation)
                .Include(c => c.Xe)
                .Include(c => c.TaiXe)
                .OrderByDescending(c => c.NgayDi)
                .ThenByDescending(c => c.GioDi)
                .ToListAsync();

            return View(chuyenXeDaPhanCong);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyPhanCong(string chuyenId)
        {
            if (string.IsNullOrEmpty(chuyenId))
            {
                TempData["ErrorMessage"] = "Thông tin không hợp lệ.";
                return RedirectToAction(nameof(LichSuPhanCong));
            }

            try
            {
                var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var admin = await _context.NguoiDung.FirstOrDefaultAsync(u => u.UserId == adminUserId);

                if (admin == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin admin.";
                    return RedirectToAction(nameof(LichSuPhanCong));
                }

                var chuyenXe = await _context.ChuyenXe
                    .FirstOrDefaultAsync(c => c.ChuyenId == chuyenId && c.TaiXeId != null);

                if (chuyenXe == null)
                {
                    TempData["ErrorMessage"] = "Chuyến xe không tồn tại hoặc chưa được phân công.";
                    return RedirectToAction(nameof(LichSuPhanCong));
                }

                var taiXe = await _context.TaiXe
                    .FirstOrDefaultAsync(t => t.UserId == chuyenXe.TaiXeId && t.AdminId == admin.UserId);

                if (taiXe == null)
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền hủy phân công chuyến xe này.";
                    return RedirectToAction(nameof(LichSuPhanCong));
                }
                if (chuyenXe.TrangThai == TrangThaiChuyenXe.DangDiChuyen ||
                    chuyenXe.TrangThai == TrangThaiChuyenXe.DaHoanThanh)
                {
                    TempData["ErrorMessage"] = "Không thể hủy phân công chuyến xe đã khởi hành hoặc hoàn thành.";
                    return RedirectToAction(nameof(LichSuPhanCong));
                }

                var oldTaiXeId = chuyenXe.TaiXeId;
                chuyenXe.TaiXeId = null;
                if (chuyenXe.TrangThai == TrangThaiChuyenXe.ChoKhoiHanh)
                {
                    chuyenXe.TrangThai = TrangThaiChuyenXe.DaLenLich;
                }

                _context.ChuyenXe.Update(chuyenXe);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Admin {admin.UserId} đã hủy phân công tài xế {oldTaiXeId} cho chuyến xe {chuyenId}");
                TempData["SuccessMessage"] = "Hủy phân công thành công!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi hủy phân công lịch");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi hủy phân công. Vui lòng thử lại.";
            }

            return RedirectToAction(nameof(LichSuPhanCong));
        }
        // GET: /TaiXe/ChinhSuaPhanCong
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChinhSuaPhanCong(string chuyenId)
        {
            if (string.IsNullOrEmpty(chuyenId))
            {
                TempData["ErrorMessage"] = "Không tìm thấy chuyến xe.";
                return RedirectToAction(nameof(LichSuPhanCong));
            }

            var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var admin = await _context.NguoiDung.FirstOrDefaultAsync(u => u.UserId == adminUserId);

            if (admin == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin admin.";
                return RedirectToAction(nameof(LichSuPhanCong));
            }

            var chuyenXe = await _context.ChuyenXe
                .Include(c => c.TaiXe)
                .Include(c => c.LoTrinh)
                    .ThenInclude(lt => lt.TramDiNavigation)
                .Include(c => c.LoTrinh)
                    .ThenInclude(lt => lt.TramToiNavigation)
                .Include(c => c.Xe)
                .FirstOrDefaultAsync(c => c.ChuyenId == chuyenId);

            if (chuyenXe == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy chuyến xe.";
                return RedirectToAction(nameof(LichSuPhanCong));
            }

            // Lấy danh sách tài xế thuộc admin
            var taiXes = await _context.TaiXe
                .Where(t => t.AdminId == admin.UserId && t.TrangThai == "Hoạt động")
                .ToListAsync();

            ViewBag.TaiXeList = new SelectList(taiXes, "UserId", "HoTen", chuyenXe.TaiXeId);

            return View(chuyenXe);
        }

        // POST: /TaiXe/ChinhSuaPhanCong
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChinhSuaPhanCong(string chuyenId, string taiXeId)
        {
            if (string.IsNullOrEmpty(chuyenId) || string.IsNullOrEmpty(taiXeId))
            {
                TempData["ErrorMessage"] = "Thông tin không hợp lệ.";
                return RedirectToAction(nameof(LichSuPhanCong));
            }

            try
            {
                var chuyenXe = await _context.ChuyenXe.FirstOrDefaultAsync(c => c.ChuyenId == chuyenId);
                if (chuyenXe == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy chuyến xe.";
                    return RedirectToAction(nameof(LichSuPhanCong));
                }

                var isTrungLich = await KiemTraTrungLich(taiXeId, chuyenXe.NgayDi, chuyenXe.GioDi, chuyenXe.GioDenDuKien);
                if (isTrungLich)
                {
                    TempData["ErrorMessage"] = "Tài xế đã có chuyến trùng lịch. Vui lòng chọn tài xế khác.";
                    return RedirectToAction(nameof(ChinhSuaPhanCong), new { chuyenId });
                }

                chuyenXe.TaiXeId = taiXeId;
                _context.ChuyenXe.Update(chuyenXe);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cập nhật phân công tài xế thành công!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi chỉnh sửa phân công tài xế");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi chỉnh sửa phân công.";
            }

            return RedirectToAction(nameof(LichSuPhanCong));
        }

        private async Task<bool> KiemTraTrungLich(string taiXeId, DateTime ngayDi, TimeSpan gioDi, TimeSpan gioDenDuKien)
        {
            try
            {
                _logger.LogInformation($"Kiểm tra trùng lịch đơn giản: taiXeId={taiXeId}, ngayDi={ngayDi:dd/MM/yyyy}");

                var coChuyenTrongNgay = await _context.ChuyenXe
                    .AnyAsync(c => c.TaiXeId == taiXeId &&
                                  c.NgayDi.Date == ngayDi.Date &&
                                  c.TrangThai != TrangThaiChuyenXe.DaHuy &&
                                  c.TrangThai != TrangThaiChuyenXe.DaHoanThanh);

                if (coChuyenTrongNgay)
                {
                    _logger.LogWarning($"Tài xế {taiXeId} đã có chuyến trong ngày {ngayDi:dd/MM/yyyy}");

                    var chuyenTrongNgay = await _context.ChuyenXe
                        .Where(c => c.TaiXeId == taiXeId && c.NgayDi.Date == ngayDi.Date)
                        .Select(c => new { c.ChuyenId, c.NgayDi, c.GioDi, c.GioDenDuKien })
                        .FirstOrDefaultAsync();

                    if (chuyenTrongNgay != null)
                    {
                        _logger.LogWarning($"Chuyến trùng: {chuyenTrongNgay.ChuyenId} - {chuyenTrongNgay.NgayDi:dd/MM/yyyy} {chuyenTrongNgay.GioDi}->{chuyenTrongNgay.GioDenDuKien}");
                    }
                }

                return coChuyenTrongNgay;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra trùng lịch đơn giản");
                return false;
            }



        }

     
        [Authorize(Roles = "TaiXe")] // Bắt buộc đăng nhập với vai trò "TaiXe"


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> KhoaTaiKhoan(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var taixe = await _context.TaiXe
                .Include(t => t.NguoiDung)
                .FirstOrDefaultAsync(t => t.UserId == id);

            if (taixe == null)
                return NotFound();

            taixe.NguoiDung.TrangThai = TrangThaiNguoiDung.BiKhoa;
            taixe.TrangThai = "Bị khóa";

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã khóa tài khoản tài xế!";
            return RedirectToAction(nameof(Index));
        }

        // ===============================
        // 🧱 MỞ KHÓA TÀI KHOẢN TÀI XẾ
        // ===============================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MoKhoaTaiKhoan(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var taixe = await _context.TaiXe
                .Include(t => t.NguoiDung)
                .FirstOrDefaultAsync(t => t.UserId == id);

            if (taixe == null)
                return NotFound();

            taixe.NguoiDung.TrangThai = TrangThaiNguoiDung.HoatDong;
            taixe.TrangThai = "Hoạt động";

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã mở khóa tài khoản tài xế!";
            return RedirectToAction(nameof(Index));
        }




        // GET: /TaiXe/DanhSachChuyen
        public async Task<IActionResult> DanhSachChuyen()
        {
            // Lấy ID của tài xế đang đăng nhập từ cookie
            var taiXeId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Nếu không tìm thấy ID (lỗi đăng nhập), trả về trang lỗi
            if (string.IsNullOrEmpty(taiXeId))
            {
                return Unauthorized("Không thể xác định thông tin tài xế.");
            }

            var homNay = DateTime.Today;
            int diff = (7 + (int)homNay.DayOfWeek - (int)DayOfWeek.Monday) % 7;
            var ngayDauTuan = homNay.AddDays(-1 * diff).Date;
            var ngayCuoiTuan = ngayDauTuan.AddDays(6).Date;
            // Truy vấn CSDL, lọc theo ĐÚNG taiXeId đã đăng nhập
            var danhSachChuyen = await _context.ChuyenXe
          .Include(c => c.LoTrinh).ThenInclude(lt => lt.TramDiNavigation)
          .Include(c => c.LoTrinh).ThenInclude(lt => lt.TramToiNavigation)
          .Where(c => c.TaiXeId == taiXeId &&
                      c.NgayDi.Date >= ngayDauTuan &&
                      c.NgayDi.Date <= ngayCuoiTuan)
          .OrderBy(c => c.NgayDi) // Sắp xếp theo ngày đi trước
          .ThenBy(c => c.GioDi)  // Sau đó sắp xếp theo giờ đi
          .ToListAsync();

            ViewBag.TuanHienTai = $"Tuần từ {ngayDauTuan:dd/MM} đến {ngayCuoiTuan:dd/MM/yyyy}";

            return View(danhSachChuyen);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckInXuatBen(string id)
        {
            var taiXeId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var chuyenXe = await _context.ChuyenXe.FirstOrDefaultAsync(c => c.ChuyenId == id && c.TaiXeId == taiXeId);

            if (chuyenXe == null)
            {
                return Forbid();
            }

            if (chuyenXe.TrangThai == TrangThaiChuyenXe.ChoKhoiHanh)
            {
            
                var gioHienTai = DateTime.Now;
                var gioKhoiHanh = chuyenXe.NgayDi.Date + chuyenXe.GioDi;

                // Cho phép check-in trong khoảng 30 phút trước và 15 phút sau giờ khởi hành
                if (gioHienTai >= gioKhoiHanh.AddMinutes(-30) && gioHienTai <= gioKhoiHanh.AddMinutes(15))
                {
                    chuyenXe.TrangThai = TrangThaiChuyenXe.DangDiChuyen;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Check-in xuất bến thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = $"Chỉ được phép check-in trong khoảng 30 phút trước giờ khởi hành ({gioKhoiHanh:HH:mm}).";
                }
                
            }
            else
            {
                TempData["ErrorMessage"] = "Trạng thái chuyến xe không hợp lệ để check-in.";
            }

            return RedirectToAction(nameof(LichLamViec)); // Đổi sang LichLamViec
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckInDenNoi(string id)
        {
            var taiXeId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var chuyenXe = await _context.ChuyenXe.FirstOrDefaultAsync(c => c.ChuyenId == id && c.TaiXeId == taiXeId);

            if (chuyenXe == null)
            {
                return Forbid();
            }

            if (chuyenXe.TrangThai == TrangThaiChuyenXe.DangDiChuyen)
            {
                chuyenXe.TrangThai = TrangThaiChuyenXe.DaHoanThanh;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xác nhận hoàn thành chuyến đi!";
            }
            else
            {
                TempData["ErrorMessage"] = "Trạng thái chuyến xe không hợp lệ.";
            }

            return RedirectToAction(nameof(LichLamViec)); 
        }

    }   
}