using AdminDashboard.Models;
using AdminDashboard.Models.Login;
using AdminDashboard.TransportDBContext;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AdminDashboard.Controllers
{
    public class AuthController : Controller
    {
        private readonly Db27524Context _context;

        public AuthController(Db27524Context context)
        {
            _context = context;
        }

        // ==============================
        //  TRANG THÔNG TIN TÀI KHOẢN
        // ==============================
        public async Task<IActionResult> Account()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Auth");

            var user = await _context.NguoiDung.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return View(new NguoiDung());

            var role = await (from ur in _context.UserRole
                              join r in _context.VaiTro on ur.RoleId equals r.RoleId
                              where ur.UserId == user.UserId
                              select r.TenVaiTro).FirstOrDefaultAsync() ?? "Khach";

            ViewData["VaiTro"] = role;
            return View(user);
        }

        // ==============================
        //  CẬP NHẬT THÔNG TIN TÀI KHOẢN (AJAX)
        // ==============================
        [HttpPost]
        public async Task<IActionResult> EditAccount([FromForm] NguoiDung model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, message = "Người dùng chưa đăng nhập." });

            var user = await _context.NguoiDung.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return NotFound(new { success = false, message = "Không tìm thấy người dùng." });

            // Cập nhật thông tin
            user.HoTen = model.HoTen;
            user.Email = model.Email;
            user.SoDienThoai = model.SoDienThoai;
            user.NgaySinh = model.NgaySinh;

            // Cập nhật qua bảng KhachHang
            var khachHang = await _context.KhachHang.FirstOrDefaultAsync(kh => kh.UserId == userId);
            if (khachHang != null)
            {
                khachHang.TenKhachHang = model.HoTen;
                khachHang.DiaChiMail = model.Email;
                khachHang.SoDienThoai = model.SoDienThoai;
                khachHang.NgaySinh = model.NgaySinh;
            }

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Cập nhật thông tin thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Lỗi khi lưu dữ liệu: {ex.Message}" });
            }
        }

        // ==============================
        //  TRANG QUÊN MẬT KHẨU
        // ==============================
        public IActionResult ForgotPass()
        {
            return View();
        }

        // ==============================
        //  ĐĂNG NHẬP
        // ==============================
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(DangNhap model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Vui lòng kiểm tra lại thông tin nhập vào.");
                return View(model);
            }

            var user = await _context.NguoiDung
                .FirstOrDefaultAsync(u => u.TenDangNhap == model.TenDangNhap && u.MatKhau == model.MatKhau);

            if (user == null)
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                return View(model);
            }

            if (user.TrangThai != "Hoạt động")
            {
                ModelState.AddModelError("", "Tài khoản của bạn hiện không hoạt động. Vui lòng liên hệ quản trị viên.");
                return View(model);
            }

            var role = await (from ur in _context.UserRole
                              join r in _context.VaiTro on ur.RoleId equals r.RoleId
                              where ur.UserId == user.UserId
                              select r.TenVaiTro).FirstOrDefaultAsync() ?? "KhachHang";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId),
                new Claim(ClaimTypes.Name, user.TenDangNhap),
                new Claim(ClaimTypes.Role, role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync("CookieAuth", claimsPrincipal);

            // Chuyển hướng theo vai trò
            if (role == "Admin")
                return RedirectToAction("Index", "Home");
            else if (role == "KhachHang")
                return RedirectToAction("Home_User", "Home_User");
            else
                return RedirectToAction("Index", "Home");
        }

        // ==============================
        //  ĐĂNG XUẤT
        // ==============================
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Login", "Auth");
        }

        // ==============================
        //  ĐĂNG KÝ
        // ==============================
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(DangKy model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Vui lòng kiểm tra lại thông tin nhập vào.");
                return View(model);
            }

            if (await _context.NguoiDung.AnyAsync(u => u.TenDangNhap == model.TenDangNhap))
            {
                ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã được sử dụng.");
                return View(model);
            }

            if (await _context.NguoiDung.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng.");
                return View(model);
            }

            // Tạo ID
            string newUserId;
            do
            {
                newUserId = "U" + Guid.NewGuid().ToString("N").Substring(0, 5);
            } while (await _context.NguoiDung.AnyAsync(u => u.UserId == newUserId));

            var user = new NguoiDung
            {
                UserId = newUserId,
                TenDangNhap = model.TenDangNhap,
                MatKhau = model.MatKhau, // Chưa mã hoá (demo)
                Email = model.Email,
                HoTen = model.HoTen,
                SoDienThoai = model.SoDienThoai,
                NgaySinh = model.NgaySinh,
                TrangThai = "Hoạt động"
            };

            _context.NguoiDung.Add(user);

            // Vai trò Khách hàng
            var roleId = await _context.VaiTro
                .Where(r => r.TenVaiTro == "KhachHang")
                .Select(r => r.RoleId)
                .FirstOrDefaultAsync();

            if (roleId == null)
            {
                ModelState.AddModelError("", "Vai trò 'KhachHang' không tồn tại trong hệ thống.");
                return View(model);
            }

            _context.UserRole.Add(new UserRole
            {
                UserId = newUserId,
                RoleId = roleId
            });

            // Tạo khách hàng tương ứng
            string newKhId;
            do
            {
                newKhId = "KH" + Guid.NewGuid().ToString("N").Substring(0, 5);
            } while (await _context.KhachHang.AnyAsync(kh => kh.IDKhachHang == newKhId));

            _context.KhachHang.Add(new KhachHang
            {
                IDKhachHang = newKhId,
                TenKhachHang = model.HoTen,
                DiaChiMail = model.Email,
                SoDienThoai = model.SoDienThoai,
                NgaySinh = model.NgaySinh,
                UserId = newUserId
            });

            await _context.SaveChangesAsync();

            // Đăng nhập luôn sau đăng ký
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId),
                new Claim(ClaimTypes.Name, user.TenDangNhap),
                new Claim(ClaimTypes.Role, "KhachHang")
            };

            var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync("CookieAuth", claimsPrincipal);

            return RedirectToAction("Account", "Auth");
        }

        // ==============================
        //  HIỂN THỊ FORM ĐẶT LẠI MẬT KHẨU
        // ==============================
        [HttpGet]
        public async Task<IActionResult> ResetPassword()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Auth");

            var user = await _context.NguoiDung.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return RedirectToAction("Login", "Auth");

            ViewBag.Phone = user.SoDienThoai ?? "(Chưa cập nhật)";
            return View();
        }

        // ==============================
        //  XỬ LÝ CẬP NHẬT MẬT KHẨU
        // ==============================
        [HttpPost]
        public async Task<IActionResult> ResetPassword(string oldPassword, string newPassword, string confirmPassword)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Auth");

            var user = await _context.NguoiDung.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                ViewBag.Error = "Không tìm thấy người dùng.";
                return View();
            }

            if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
                ViewBag.Phone = user.SoDienThoai;
                return View();
            }

            if (user.MatKhau != oldPassword)
            {
                ViewBag.Error = "Mật khẩu cũ không đúng.";
                ViewBag.Phone = user.SoDienThoai;
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";
                ViewBag.Phone = user.SoDienThoai;
                return View();
            }

            user.MatKhau = newPassword;

            try
            {
                await _context.SaveChangesAsync();
                ViewBag.Success = "Đổi mật khẩu thành công!";
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Lỗi khi lưu: {ex.Message}";
            }

            ViewBag.Phone = user.SoDienThoai;
            return View();
        }
    }
}
