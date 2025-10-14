using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using AdminDashboard.Models;
using AdminDashboard.ViewModels;
using AdminDashboard.TransportDBContext;
using Microsoft.AspNetCore.Authorization;

namespace AdminDashboard.Controllers
{
    [Authorize(Roles = "Admin")]
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

                 var userId = await GenerateUserId();
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

                 var roleId = "R3";
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
        // Tạo mã UserId tự động
        private async Task<string> GenerateUserId()
        {
            var lastUser = await _context.NguoiDung
                .OrderByDescending(u => u.UserId)
                .FirstOrDefaultAsync();

            if (lastUser == null) return "U1001";
            if (int.TryParse(lastUser.UserId.Substring(1), out int lastNumber))
                return $"U{lastNumber + 1}";
            return "U1001";
        }

        // GET: /TaiXe/LichLamViec
        public async Task<IActionResult> LichLamViec()
        {
            // Lấy ID của tài xế đang đăng nhập
            var idTaiXe = User.FindFirstValue(ClaimTypes.NameIdentifier);

            //if (string.IsNullOrEmpty(idTaiXe))
            //{
            //    return Unauthorized(); // Không tìm thấy thông tin đăng nhập
            //}

            // Lấy danh sách các chuyến xe được phân công cho tài xế này
            // Chỉ lấy các chuyến từ hôm nay trở về sau
            var lichCuaToi = await _context.ChuyenXe
                .Where(c => c.TaiXeId == idTaiXe && c.NgayDi >= DateTime.Today)
                .Include(c => c.LoTrinh.TramDiNavigation)
                .Include(c => c.LoTrinh.TramToiNavigation)
                .Include(c => c.Xe)
                .OrderBy(c => c.NgayDi).ThenBy(c => c.GioDi) // Sắp xếp theo thời gian
                .ToListAsync();

            return View(lichCuaToi);
        }
    }
}
