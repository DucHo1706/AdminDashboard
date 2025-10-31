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

                if (await _context.NguoiDung.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email đã tồn tại.");
                    return View(model);
                }

                var userId = GenerateUserId();
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

                _logger.LogInformation($"Tạo người dùng: {userId}, Email: {model.Email}");

                _context.NguoiDung.Add(nguoiDung);
                await _context.SaveChangesAsync(); // Save ngay để có UserId

                _logger.LogInformation("Đã tạo người dùng thành công");

                var roleId = "29cf77eb-dbda-4bf4-be3e-131265a2dc37";
                var taiXeRole = await _context.VaiTro.FirstOrDefaultAsync(r => r.RoleId == roleId);
                if (taiXeRole == null)
                {
                    _logger.LogInformation("Tạo vai trò tài xế mới: R3");
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
                await _context.SaveChangesAsync();

                _logger.LogInformation("Đã gán vai trò tài xế");

                var taiXe = new TaiXe
                {
                    UserId = userId,

                    AdminId = admin.UserId,
                    BangLaiXe = model.BangLaiXe.Trim(),
                    NgayVaoLam = model.NgayVaoLam ?? DateTime.Now,
                    HoTen = model.HoTen.Trim(),
                    TrangThai = "Hoạt động"
                };




                _logger.LogInformation($"Tạo tài xế: UserId={userId}, AdminId={admin.UserId}");

                _context.TaiXe.Add(taiXe);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Đăng ký tài xế thành công");

                TempData["SuccessMessage"] = "Đăng ký tài xế thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Lỗi DbUpdateException khi đăng ký tài xế");

                if (dbEx.InnerException != null)
                {
                    _logger.LogError(dbEx.InnerException, "Inner Exception chi tiết:");
                    ViewBag.ErrorDetail = dbEx.InnerException.Message;
                }

                if (dbEx.InnerException != null)
                {
                    var innerMessage = dbEx.InnerException.Message;

                    if (innerMessage.Contains("FK_") && innerMessage.Contains("AdminId"))
                    {
                        ModelState.AddModelError("", "Lỗi: AdminId không tồn tại trong hệ thống.");
                    }
                    else if (innerMessage.Contains("FK_") && innerMessage.Contains("UserId"))
                    {
                        ModelState.AddModelError("", "Lỗi tham chiếu UserId. Vui lòng thử lại.");
                    }
                    else if (innerMessage.Contains("Cannot insert duplicate key"))
                    {
                        ModelState.AddModelError("", "Lỗi trùng lặp dữ liệu. Có thể UserId đã tồn tại.");
                    }
                    else if (innerMessage.Contains("String or binary data would be truncated"))
                    {
                        ModelState.AddModelError("", "Lỗi: Dữ liệu quá dài so với quy định của database.");
                    }
                    else
                    {
                        ModelState.AddModelError("", $"Lỗi database: {dbEx.InnerException.Message}");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Lỗi cơ sở dữ liệu khi đăng ký tài xế.");
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi tổng quát khi đăng ký tài xế");
                ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}. Vui lòng thử lại.");
                return View(model);
            }
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
        public async Task<IActionResult> LichLamViec(DateTime? tuNgay, DateTime? denNgay)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin người dùng. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var userRoles = await _context.UserRole
                   .Where(ur => ur.UserId == currentUserId)
                   .Join(_context.VaiTro, ur => ur.RoleId, r => r.RoleId, (ur, r) => r.TenVaiTro)
                   .ToListAsync();

                bool isAdmin = userRoles.Contains("Admin");
                bool isTaiXe = userRoles.Contains("TaiXe");

                if (!isAdmin && !isTaiXe)
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền truy cập tính năng này.";
                    return RedirectToAction("AccessDenied", "Auth");
                }
                var startDate = tuNgay ?? DateTime.Today;
                var endDate = denNgay ?? DateTime.Today.AddDays(7);

                IQueryable<ChuyenXe> query = _context.ChuyenXe
                   .Where(c => c.NgayDi >= startDate && c.NgayDi <= endDate)
                   .Include(c => c.LoTrinh.TramDiNavigation)
                   .Include(c => c.LoTrinh.TramToiNavigation)
                   .Include(c => c.Xe);

                if (isTaiXe && !isAdmin)
                {
                    query = query.Where(c => c.TaiXeId == currentUserId);
                }

                var lichCuaToi = await query
                    .OrderBy(c => c.NgayDi).ThenBy(c => c.GioDi)
                    .ToListAsync();

                ViewBag.TuNgay = startDate.ToString("yyyy-MM-dd");
                ViewBag.DenNgay = endDate.ToString("yyyy-MM-dd");
                ViewBag.IsAdmin = isAdmin;
                ViewBag.IsTaiXe = isTaiXe;

                return View(lichCuaToi);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải lịch làm việc");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải dữ liệu.";
                return RedirectToAction("Index", "Home");
            }
        }

        [Authorize(Roles = "Admin,TaiXe")]
        public async Task<IActionResult> GetChuyenDetail(string chuyenId)
        {
            if (string.IsNullOrEmpty(chuyenId))
            {
                return Content("<div class='alert alert-danger'>Không tìm thấy thông tin chuyến xe.</div>");
            }

            var chuyenXe = await _context.ChuyenXe
                .Include(c => c.LoTrinh.TramDiNavigation)
                .Include(c => c.LoTrinh.TramToiNavigation)
                .Include(c => c.Xe)
                .Include(c => c.TaiXe)
                .FirstOrDefaultAsync(c => c.ChuyenId == chuyenId);

            if (chuyenXe == null)
            {
                return Content("<div class='alert alert-danger'>Không tìm thấy chuyến xe.</div>");
            }

            var taiXeInfo = await _context.TaiXe
               .FirstOrDefaultAsync(t => t.UserId == chuyenXe.TaiXeId);

            var soGhe = 0;
            if (chuyenXe.XeId != null)
            {
                soGhe = await _context.Ghe
                    .CountAsync(g => g.XeId == chuyenXe.XeId);
            }

            var htmlContent = $@"
        <div class='row'>
            <div class='col-md-6'>
                <h6>Thông tin chuyến</h6>
                <p><strong>Mã chuyến:</strong> {chuyenXe.ChuyenId}</p>
                <p><strong>Ngày đi:</strong> {chuyenXe.NgayDi:dd/MM/yyyy}</p>
                <p><strong>Giờ đi:</strong> {chuyenXe.GioDi:hh\\:mm}</p>
                <p><strong>Giờ đến dự kiến:</strong> {chuyenXe.GioDenDuKien:hh\\:mm}</p>
                <p><strong>Trạng thái:</strong> <span class='badge badge-primary'>{chuyenXe.TrangThai}</span></p>
            </div>
            <div class='col-md-6'>
                <h6>Thông tin lộ trình</h6>
                <p><strong>Điểm đi:</strong> {chuyenXe.LoTrinh?.TramDiNavigation?.TenTram ?? "N/A"}</p>
                <p><strong>Điểm đến:</strong> {chuyenXe.LoTrinh?.TramToiNavigation?.TenTram ?? "N/A"}</p>
                <p><strong>Giá vé:</strong> {(chuyenXe.LoTrinh?.GiaVeCoDinh?.ToString("C0") ?? "N/A")}</p>
            </div>
        </div>
        <div class='row mt-3'>
            <div class='col-md-6'>
                <h6>Thông tin xe</h6>
                <p><strong>Biển số:</strong> {chuyenXe.Xe?.BienSoXe ?? "N/A"}</p>
                <p><strong>Tổng số ghế:</strong> {soGhe}</p>
            </div>
            <div class='col-md-6'>
                <h6>Thông tin tài xế</h6>
                <p><strong>Họ tên:</strong> {chuyenXe.TaiXe?.HoTen ?? "N/A"}</p>";

            if (taiXeInfo != null)
            {
                htmlContent += $@"<p><strong>Bằng lái:</strong> {taiXeInfo.BangLaiXe}</p>";
            }

            htmlContent += $@"
            </div>
        </div>";

            return Content(htmlContent);
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> XemLichTaiXe(string taiXeId)
        {
            if (string.IsNullOrEmpty(taiXeId))
            {
                return Content("<div class='alert alert-danger'>Không tìm thấy thông tin tài xế.</div>");
            }

            try
            {
                var lichTaiXe = await _context.ChuyenXe
                    .Where(c => c.TaiXeId == taiXeId && c.TrangThai != TrangThaiChuyenXe.DaHuy)
                    .Include(c => c.LoTrinh.TramDiNavigation)
                    .Include(c => c.LoTrinh.TramToiNavigation)
                    .Include(c => c.Xe)
                    .OrderBy(c => c.NgayDi)
                    .ThenBy(c => c.GioDi)
                    .ToListAsync();

                if (!lichTaiXe.Any())
                {
                    return Content("<div class='alert alert-info'>Tài xế chưa có chuyến xe nào được phân công.</div>");
                }

                var html = "<table class='table table-bordered table-sm'>";
                html += "<thead><tr><th>Ngày đi</th><th>Giờ đi</th><th>Giờ đến</th><th>Điểm đi</th><th>Điểm đến</th><th>Xe</th><th>Trạng thái</th></tr></thead><tbody>";

                foreach (var c in lichTaiXe)
                {
                    html += $"<tr>" +
                            $"<td>{c.NgayDi:dd/MM/yyyy}</td>" +
                            $"<td>{c.GioDi:hh\\:mm}</td>" +
                            $"<td>{c.GioDenDuKien:hh\\:mm}</td>" +
                            $"<td>{c.LoTrinh?.TramDiNavigation?.TenTram ?? "N/A"}</td>" +
                            $"<td>{c.LoTrinh?.TramToiNavigation?.TenTram ?? "N/A"}</td>" +
                            $"<td>{c.Xe?.BienSoXe ?? "N/A"}</td>" +
                            $"<td>{c.TrangThai}</td>" +
                            $"</tr>";
                }

                html += "</tbody></table>";
                return Content(html);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xem lịch tài xế");
                return Content("<div class='alert alert-danger'>Có lỗi xảy ra khi tải lịch tài xế.</div>");
            }
        }


    }
}