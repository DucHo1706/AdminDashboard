using AdminDashboard.Models;
using AdminDashboard.Models.Login;
using AdminDashboard.TransportDBContext;
using AdminDashboard.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using System; // Guid
using System.Collections.Generic;

namespace AdminDashboard.Controllers
{
    public class AuthController : Controller
    {
        private readonly Db27524Context _context;
        private readonly IEmailService _emailService;

        public AuthController(Db27524Context context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // ====== Login / Logout / Register (giữ nguyên phần logic bạn đã có) ======
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(DangNhap model)
        {
            if (!ModelState.IsValid) return View(model);

            var input = model.EmailOrPhone?.Trim().ToLowerInvariant() ?? string.Empty;
            var password = model.MatKhau ?? string.Empty;

            if (string.IsNullOrWhiteSpace(input))
            {
                ModelState.AddModelError("", "Vui lòng nhập email hoặc số điện thoại.");
                return View(model);
            }

            var user = await _context.NguoiDung
                .FirstOrDefaultAsync(u =>
                    (u.Email.ToLower() == input || u.SoDienThoai == input) && u.MatKhau == password);

            if (user == null)
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                return View(model);
            }

            if (user.TrangThai != TrangThaiNguoiDung.HoatDong)
            {
                ModelState.AddModelError("", "Tài khoản của bạn đã bị khóa.");
                return View(model);
            }

            var roleName = await _context.UserRole
                .Where(ur => ur.UserId == user.UserId)
                .Join(_context.VaiTro, ur => ur.RoleId, r => r.RoleId, (ur, r) => r.TenVaiTro)
                .FirstOrDefaultAsync() ?? "Khach";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, roleName)
            };

            var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await HttpContext.SignInAsync("CookieAuth", claimsPrincipal);

            if (roleName == "Admin")
                return RedirectToAction("Index", "Home");
            else if (roleName == "TaiXe")
                return RedirectToAction("LichLamViec", "TaiXe");
            else
                return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Login", "Auth");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(DangKy model)
        {
            if (!ModelState.IsValid) return View(model);

            if (await _context.NguoiDung.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                return View(model);
            }

            var newUserId = Guid.NewGuid().ToString();

            var user = new NguoiDung
            {
                UserId = newUserId,
                HoTen = model.HoTen,
                Email = model.Email,
                SoDienThoai = model.SoDienThoai,
                NgaySinh = model.NgaySinh,
                TrangThai = TrangThaiNguoiDung.HoatDong,
                MatKhau = model.MatKhau
            };
            _context.NguoiDung.Add(user);

            const string defaultRoleName = "KhachHang";
            var role = await _context.VaiTro.FirstOrDefaultAsync(r => r.TenVaiTro == defaultRoleName);

            if (role == null)
            {
                role = new VaiTro
                {
                    RoleId = Guid.NewGuid().ToString(),
                    TenVaiTro = defaultRoleName
                };
                _context.VaiTro.Add(role);
            }

            _context.UserRole.Add(new UserRole { UserId = newUserId, RoleId = role.RoleId });

            await _context.SaveChangesAsync();

            // Tự động đăng nhập
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, defaultRoleName)
            };
            var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await HttpContext.SignInAsync("CookieAuth", claimsPrincipal);

            return RedirectToAction("Account", "Auth");
        }

        // ====== Account view ======
        [HttpGet]
        public async Task<IActionResult> Account()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Auth");

            var user = await _context.NguoiDung.FirstOrDefaultAsync(u => u.UserId == userId);
            return View(user);
        }

        // ====== EDIT ACCOUNT: GET (truy cập trang chỉnh sửa nếu cần) ======
        [HttpGet]
        public async Task<IActionResult> EditAccount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Auth");

            var user = await _context.NguoiDung.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return NotFound();

            return View(user); // view EditAccount.cshtml dùng model NguoiDung (như bạn đã có)
        }

        // ====== EDIT ACCOUNT: POST (cập nhật) ======
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAccount(NguoiDung model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var user = await _context.NguoiDung.FindAsync(model.UserId);
            if (user == null)
            {
                return Json(new { success = false, message = "Không tìm thấy tài khoản." });
            }

            user.HoTen = model.HoTen;
            user.Email = model.Email;
            user.SoDienThoai = model.SoDienThoai;
            user.NgaySinh = model.NgaySinh;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Cập nhật thành công." });
        }

        // Helper: kiểm tra AJAX
        private bool IsAjaxRequest()
        {
            return Request.Headers != null && Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        // ====== RESET PASSWORD ======
        [HttpGet]
        public async Task<IActionResult> ResetPassword()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _context.NguoiDung.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.Phone = user.SoDienThoai ?? "Không có số điện thoại";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string oldPassword, string newPassword, string confirmPassword)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _context.NguoiDung.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.Phone = user.SoDienThoai ?? "Không có số điện thoại";

            // Kiểm tra mật khẩu cũ có đúng không
            if (user.MatKhau != oldPassword)
            {
                ViewBag.Error = "Mật khẩu cũ không chính xác.";
                return View();
            }

            // Kiểm tra mật khẩu mới có khớp xác nhận không
            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";
                return View();
            }

            // Kiểm tra độ dài hoặc độ mạnh của mật khẩu (tuỳ chọn)
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                ViewBag.Error = "Mật khẩu mới phải có ít nhất 6 ký tự.";
                return View();
            }

            // ✅ Cập nhật mật khẩu mới
            user.MatKhau = newPassword;

            try
            {
                await _context.SaveChangesAsync();
                ViewBag.Success = "Đặt lại mật khẩu thành công!";
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Đã xảy ra lỗi khi lưu: " + ex.Message;
            }

            return View();
        }
        // ====== FORGOT PASSWORD ======
        [HttpGet]
        public IActionResult ForgotPass()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPass(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("", "Vui lòng nhập email.");
                return View();
            }

            // Kiểm tra email có tồn tại trong hệ thống không
            var user = await _context.NguoiDung.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
            if (user == null)
            {
                ModelState.AddModelError("", "Email không tồn tại trong hệ thống.");
                return View();
            }

            // Tạo mã OTP ngẫu nhiên 6 chữ số
            var random = new Random();
            var otpCode = random.Next(100000, 999999).ToString();

            // Lưu OTP vào database
            var otpVerification = new OtpVerification
            {
                Email = email.ToLower(),
                OtpCode = otpCode,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddMinutes(10), // OTP có hiệu lực 10 phút
                IsUsed = false,
                UserId = user.UserId
            };

            // Xóa các OTP cũ của email này
            var oldOtps = await _context.OtpVerification
                .Where(o => o.Email == email.ToLower() && !o.IsUsed)
                .ToListAsync();
            _context.OtpVerification.RemoveRange(oldOtps);

            _context.OtpVerification.Add(otpVerification);
            await _context.SaveChangesAsync();

            // Gửi email OTP
            var emailSent = await _emailService.SendOtpEmailAsync(email, otpCode, user.HoTen);
            if (!emailSent)
            {
                ModelState.AddModelError("", "Không thể gửi email. Vui lòng thử lại sau.");
                return View();
            }

            // Chuyển đến trang nhập OTP
            ViewBag.Email = email;
            return RedirectToAction("VerifyOtp", new { email = email });
        }

        [HttpGet]
        public IActionResult VerifyOtp(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return RedirectToAction("ForgotPass");
            }

            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOtp(string email, string otpCode)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(otpCode))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin.");
                ViewBag.Email = email;
                return View();
            }

            // Tìm OTP trong database
            var otpVerification = await _context.OtpVerification
                .FirstOrDefaultAsync(o => o.Email == email.ToLower() && 
                                         o.OtpCode == otpCode && 
                                         !o.IsUsed && 
                                         o.ExpiresAt > DateTime.Now);

            if (otpVerification == null)
            {
                ModelState.AddModelError("", "Mã OTP không hợp lệ hoặc đã hết hạn.");
                ViewBag.Email = email;
                return View();
            }

            // Đánh dấu OTP đã được sử dụng
            otpVerification.IsUsed = true;
            await _context.SaveChangesAsync();

            // Chuyển đến trang đặt lại mật khẩu
            ViewBag.Email = email;
            return RedirectToAction("SetNewPassword", new { email = email });
        }

        [HttpGet]
        public IActionResult SetNewPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return RedirectToAction("ForgotPass");
            }

            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SetNewPassword(string email, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin.");
                ViewBag.Email = email;
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu xác nhận không khớp.");
                ViewBag.Email = email;
                return View();
            }

            if (newPassword.Length < 6)
            {
                ModelState.AddModelError("", "Mật khẩu phải có ít nhất 6 ký tự.");
                ViewBag.Email = email;
                return View();
            }

            // Tìm user theo email
            var user = await _context.NguoiDung.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
            if (user == null)
            {
                ModelState.AddModelError("", "Không tìm thấy tài khoản.");
                ViewBag.Email = email;
                return View();
            }

            // Cập nhật mật khẩu mới
            user.MatKhau = newPassword;
            await _context.SaveChangesAsync();

            // Xóa tất cả OTP của email này
            var otps = await _context.OtpVerification
                .Where(o => o.Email == email.ToLower())
                .ToListAsync();
            _context.OtpVerification.RemoveRange(otps);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đặt lại mật khẩu thành công! Bạn có thể đăng nhập với mật khẩu mới.";
            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> ResendOtp(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Json(new { success = false, message = "Email không hợp lệ." });
            }

            // Kiểm tra email có tồn tại trong hệ thống không
            var user = await _context.NguoiDung.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
            if (user == null)
            {
                return Json(new { success = false, message = "Email không tồn tại trong hệ thống." });
            }

            // Tạo mã OTP mới
            var random = new Random();
            var otpCode = random.Next(100000, 999999).ToString();

            // Xóa các OTP cũ của email này
            var oldOtps = await _context.OtpVerification
                .Where(o => o.Email == email.ToLower())
                .ToListAsync();
            _context.OtpVerification.RemoveRange(oldOtps);

            // Lưu OTP mới
            var otpVerification = new OtpVerification
            {
                Email = email.ToLower(),
                OtpCode = otpCode,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddMinutes(10),
                IsUsed = false,
                UserId = user.UserId
            };

            _context.OtpVerification.Add(otpVerification);
            await _context.SaveChangesAsync();

            // Gửi email OTP
            var emailSent = await _emailService.SendOtpEmailAsync(email, otpCode, user.HoTen);
            if (!emailSent)
            {
                return Json(new { success = false, message = "Không thể gửi email. Vui lòng thử lại sau." });
            }

            return Json(new { success = true, message = "Mã OTP mới đã được gửi!" });
        }
        // ====== HISTORY ======
        public async Task<IActionResult> History()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            // Giữ nguyên các đơn hết hạn ở trạng thái Chờ thanh toán để hiển thị bên "Hiện tại".
            // Không auto-cancel và không giải phóng ghế tại đây; việc hủy sẽ do người dùng hoặc tác vụ khác xử lý.

            var donHangs = await _context.DonHang
                .Where(d => d.IDKhachHang == userId)
                .Include(d => d.ChuyenXe).ThenInclude(cx => cx.LoTrinh).ThenInclude(lt => lt.TramDiNavigation)
                .Include(d => d.ChuyenXe).ThenInclude(cx => cx.LoTrinh).ThenInclude(lt => lt.TramToiNavigation)
                .OrderByDescending(d => d.NgayDat)
                .ToListAsync();

            return View(donHangs);
        }
    }
}