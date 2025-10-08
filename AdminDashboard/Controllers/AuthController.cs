using AdminDashboard.Models;
using AdminDashboard.Models.Login;
using AdminDashboard.TransportDBContext;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Linq;

namespace AdminDashboard.Controllers
{
    public class AuthController : Controller
    {
        private readonly Db27524Context _context;

        public AuthController(Db27524Context context)
        {
            _context = context;
        }
        public IActionResult ForgotPass()
        {
            return View();
        }

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

            var inputRaw = model.EmailOrPhone?.Trim() ?? string.Empty;
            var password = model.MatKhau?.Trim() ?? string.Empty;

            NguoiDung? user = null;

            if (string.IsNullOrWhiteSpace(inputRaw))
            {
                ModelState.AddModelError("", "Vui lòng nhập email hoặc số điện thoại.");
                return View(model);
            }

            bool looksLikeEmail = inputRaw.Contains("@");

            if (looksLikeEmail)
            {
                var email = inputRaw.ToLowerInvariant();
                user = await _context.NguoiDung
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email && u.MatKhau == password);
            }
            else
            {
                // Normalize phone: keep digits only, handle VN formats 0xxxxxxxxx / 84xxxxxxxxx / +84xxxxxxxxx
                var digitsOnly = Regex.Replace(inputRaw, "\\D", "");
                var candidates = new List<string>();

                if (digitsOnly.StartsWith("84"))
                {
                    var local = digitsOnly.Length > 2 ? ("0" + digitsOnly.Substring(2)) : digitsOnly;
                    candidates.Add(local);
                    candidates.Add(digitsOnly);
                    candidates.Add("+" + digitsOnly);
                }
                else if (digitsOnly.StartsWith("0"))
                {
                    candidates.Add(digitsOnly);
                    var intl = digitsOnly.Length > 1 ? ("84" + digitsOnly.Substring(1)) : digitsOnly;
                    candidates.Add(intl);
                    candidates.Add("+" + intl);
                }
                else
                {
                    // Fallback: try as-is and with common VN prefixes
                    candidates.Add(digitsOnly);
                    candidates.Add("0" + digitsOnly);
                    candidates.Add("84" + digitsOnly);
                    candidates.Add("+84" + digitsOnly);
                }

                var kh = await _context.KhachHang
                    .FirstOrDefaultAsync(k => k.SoDienThoai != null && candidates.Contains(k.SoDienThoai));

                if (kh != null && !string.IsNullOrEmpty(kh.UserId))
                {
                    user = await _context.NguoiDung
                        .FirstOrDefaultAsync(u => u.UserId == kh.UserId && u.MatKhau == password);
                }
            }

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
                              select r.TenVaiTro).FirstOrDefaultAsync() ?? "Khach";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync("CookieAuth", claimsPrincipal);

            // Chuyển hướng dựa trên vai trò
            if (role == "Admin")
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            else if (role == "KhachHang")
            {
                return RedirectToAction("Home_User", "Home_User");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
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
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Vui lòng kiểm tra lại thông tin nhập vào.");
                return View(model);
            }

            // Kiểm tra email đã tồn tại
            if (await _context.NguoiDung.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng.");
                return View(model);
            }

            // Tạo UserId mới
            string newUserId;
            do
            {
                newUserId = "U" + Guid.NewGuid().ToString("N").Substring(0, 5);
            } while (await _context.NguoiDung.AnyAsync(u => u.UserId == newUserId));

            var user = new NguoiDung
            {
                UserId = newUserId,
                MatKhau = model.MatKhau, // Lưu plaintext như yêu cầu (KHÔNG AN TOÀN)
                Email = model.Email,
                HoTen = model.HoTen,
                SoDienThoai = model.SoDienThoai,
                NgaySinh = model.NgaySinh,
                TrangThai = "Hoạt động"
            };

            _context.NguoiDung.Add(user);

            // Gán role mặc định là "KhachHang"
            var roleId = await _context.VaiTro
                .Where(r => r.TenVaiTro == "KhachHang")
                .Select(r => r.RoleId)
                .FirstOrDefaultAsync();

            if (roleId == null)
            {
                ModelState.AddModelError("", "Vai trò KhachHang không tồn tại trong hệ thống.");
                return View(model);
            }

            _context.UserRole.Add(new UserRole
            {
                UserId = newUserId,
                RoleId = roleId
            });

            // Tạo KhachHang mapping
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

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi khi lưu dữ liệu: {ex.Message}");
                return View(model);
            }

            // Tự động đăng nhập sau đăng ký
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, "KhachHang")
            };

            var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync("CookieAuth", claimsPrincipal);

            // Chuyển hướng đến trang thông tin tài khoản
            return RedirectToAction("Account", "Auth");
        }


        [HttpGet]
        public async Task<IActionResult> Account()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.NguoiDung.FirstOrDefaultAsync(u => u.UserId == userId);
            return View(user);
        }





    }
}