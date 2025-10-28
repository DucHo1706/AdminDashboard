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
<<<<<<< HEAD
using BCrypt.Net; 
=======

>>>>>>> master
namespace AdminDashboard.Controllers
{
    public class AuthController : Controller
    {
        private readonly Db27524Context _context;
        private readonly IEmailService _emailService;
<<<<<<< HEAD
        private readonly IOtpService _otpService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(Db27524Context context, IEmailService emailService, IOtpService otpService, ILogger<AuthController> logger)
        {
            _context = context;
            _emailService = emailService;
            _otpService = otpService;
            _logger = logger;
=======

        public AuthController(Db27524Context context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
>>>>>>> master
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
<<<<<<< HEAD
        [HttpGet]
=======
>>>>>>> master
        public IActionResult ForgotPass()
        {
            return View();
        }
<<<<<<< HEAD
        public IActionResult History()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPass(ForgotPasswordRequest model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Kiểm tra email có tồn tại trong hệ thống không
            var user = await _context.NguoiDung
                .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());

            if (user == null)
            {
                ModelState.AddModelError("Email", "Email này không tồn tại trong hệ thống.");
                return View(model);
            }

            try
            {
                // Tạo mã OTP
                var otpCode = await _otpService.GenerateOtpAsync(model.Email);

                // Gửi email OTP
                var emailSent = await _emailService.SendOtpEmailAsync(model.Email, otpCode);

                if (emailSent)
                {
                    // Lưu thời gian tạo OTP vào TempData
                    var otpCreatedTime = DateTime.Now;
                    TempData["OtpCreatedTime"] = otpCreatedTime.ToString("O");
                    TempData["OtpExpiresIn"] = 180; // 3 phút = 180 giây

                    TempData["SuccessMessage"] = "Mã OTP đã được gửi đến email của bạn. Vui lòng kiểm tra hộp thư.";
                    return RedirectToAction("VerifyOtp", new { email = model.Email });
                }
                else
                {
                    ModelState.AddModelError("", "Không thể gửi email. Vui lòng thử lại sau.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ForgotPass action");
                ModelState.AddModelError("", "Đã xảy ra lỗi. Vui lòng thử lại sau.");
                return View(model);
            }
        }

        // ====== VERIFY OTP ======
        [HttpGet]
        public IActionResult VerifyOtp(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("ForgotPass");
            }

            var model = new VerifyOtpRequest { Email = email };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOtp(VerifyOtpRequest model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var isValid = await _otpService.VerifyOtpAsync(model.Email, model.OtpCode);

                if (isValid)
                {
                    TempData["SuccessMessage"] = "Mã OTP hợp lệ. Vui lòng đặt lại mật khẩu mới.";
                    return RedirectToAction("ResetPasswordWithOtp", new { email = model.Email, otpCode = model.OtpCode });
                }
                else
                {
                    ModelState.AddModelError("OtpCode", "Mã OTP không đúng hoặc đã hết hạn.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in VerifyOtp action");
                ModelState.AddModelError("", "Đã xảy ra lỗi. Vui lòng thử lại sau.");
                return View(model);
            }
        }

        // ====== RESET PASSWORD WITH OTP ======
        [HttpGet]
        public IActionResult ResetPasswordWithOtp(string email, string otpCode)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(otpCode))
            {
                return RedirectToAction("ForgotPass");
            }

            var model = new ResetPasswordRequest 
            { 
                Email = email, 
                OtpCode = otpCode 
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPasswordWithOtp(ResetPasswordRequest model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Xác minh lại OTP
                var isValidOtp = await _otpService.VerifyOtpAsync(model.Email, model.OtpCode);

                if (!isValidOtp)
                {
                    ModelState.AddModelError("", "Mã OTP không hợp lệ hoặc đã hết hạn.");
                    return View(model);
                }

                // Tìm user và cập nhật mật khẩu
                var user = await _context.NguoiDung
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());

                if (user == null)
                {
                    ModelState.AddModelError("", "Không tìm thấy tài khoản.");
                    return View(model);
                }

                // Cập nhật mật khẩu mới
                user.MatKhau = model.NewPassword;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đặt lại mật khẩu thành công! Bạn có thể đăng nhập với mật khẩu mới.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ResetPasswordWithOtp action");
                ModelState.AddModelError("", "Đã xảy ra lỗi khi đặt lại mật khẩu. Vui lòng thử lại sau.");
                return View(model);
            }
=======

        // ====== OTP FORGOT PASSWORD ======
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Error = "Vui lòng nhập email.";
                return View();
            }

            // Kiểm tra email có tồn tại trong hệ thống không
            var user = await _context.NguoiDung.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                ViewBag.Error = "Email không tồn tại trong hệ thống.";
                return View();
            }

            // Tạo mã OTP 6 chữ số
            var otpCode = new Random().Next(100000, 999999).ToString();

            // Lưu mã OTP vào database
            var otpRecord = new OtpCode
            {
                Email = email,
                Code = otpCode,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddMinutes(10), // OTP có hiệu lực 10 phút
                IsUsed = false
            };

            _context.OtpCode.Add(otpRecord);
            await _context.SaveChangesAsync();

            // Gửi email OTP
            var emailSent = await _emailService.SendOtpEmailAsync(email, otpCode);
            if (!emailSent)
            {
                ViewBag.Error = "Không thể gửi email. Vui lòng thử lại sau.";
                return View();
            }

            ViewBag.Success = "Mã OTP đã được gửi đến email của bạn.";
            ViewBag.Email = email;
            return View();
        }

        [HttpGet]
        public IActionResult VerifyOtp(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return RedirectToAction("ForgotPassword");
            }

            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOtp(string email, string otpCode)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(otpCode))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
                ViewBag.Email = email;
                return View();
            }

            // Kiểm tra mã OTP
            var otpRecord = await _context.OtpCode
                .FirstOrDefaultAsync(o => o.Email == email && o.Code == otpCode && !o.IsUsed);

            if (otpRecord == null)
            {
                ViewBag.Error = "Mã OTP không đúng hoặc đã được sử dụng.";
                ViewBag.Email = email;
                return View();
            }

            if (otpRecord.ExpiresAt < DateTime.Now)
            {
                ViewBag.Error = "Mã OTP đã hết hạn.";
                ViewBag.Email = email;
                return View();
            }

            // Đánh dấu mã OTP đã được sử dụng
            otpRecord.IsUsed = true;
            otpRecord.UsedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            // Chuyển đến trang đặt lại mật khẩu
            return RedirectToAction("ResetPasswordWithOtp", new { email = email });
        }

        [HttpGet]
        public IActionResult ResetPasswordWithOtp(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return RedirectToAction("ForgotPassword");
            }

            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPasswordWithOtp(string email, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return RedirectToAction("ForgotPassword");
            }

            ViewBag.Email = email;

            if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";
                return View();
            }

            if (newPassword.Length < 6)
            {
                ViewBag.Error = "Mật khẩu phải có ít nhất 6 ký tự.";
                return View();
            }

            // Tìm người dùng
            var user = await _context.NguoiDung.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                ViewBag.Error = "Không tìm thấy tài khoản.";
                return View();
            }

            // Kiểm tra xem có mã OTP hợp lệ đã được sử dụng gần đây không (trong vòng 30 phút)
            var recentOtp = await _context.OtpCode
                .FirstOrDefaultAsync(o => o.Email == email && o.IsUsed && o.UsedAt.HasValue && 
                    o.UsedAt.Value.AddMinutes(30) > DateTime.Now);

            if (recentOtp == null)
            {
                ViewBag.Error = "Phiên đặt lại mật khẩu đã hết hạn. Vui lòng thực hiện lại từ đầu.";
                return View();
            }

            // Cập nhật mật khẩu mới
            user.MatKhau = newPassword;
            await _context.SaveChangesAsync();

            ViewBag.Success = "Đặt lại mật khẩu thành công! Bạn có thể đăng nhập với mật khẩu mới.";
            return View();
>>>>>>> master
        }
    }
}