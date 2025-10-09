using AdminDashboard.Models;
using AdminDashboard.Models.Login;
using AdminDashboard.TransportDBContext;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using System; // Thêm thư viện để dùng Guid

namespace AdminDashboard.Controllers
{
    public class AuthController : Controller
    {
        private readonly Db27524Context _context;

        public AuthController(Db27524Context context)
        {
            _context = context;
        }

        // ... (Các action Login, Logout, ForgotPass giữ nguyên)

        #region Giữ nguyên các action khác
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
                return View(model);
            }

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
            {
                return RedirectToAction("Index", "Home"); // Hoặc trang Dashboard của Admin
            }
            else if (roleName == "TaiXe")
            {
                return RedirectToAction("LichLamViec", "TaiXe"); // Chuyển đến trang lịch làm việc
            }
            else // Mặc định là KhachHang
            {
                return RedirectToAction("Index", "Home"); // Trang chủ cho khách
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
        #endregion

        // === PHẦN SỬA LỖI NẰM Ở ĐÂY ===
        [HttpPost]
        public async Task<IActionResult> Register(DangKy model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (await _context.NguoiDung.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                return View(model);
            }

            // Dùng Guid để tạo UserId chắc chắn không trùng lặp
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

            // Nếu vai trò "KhachHang" không tồn tại, thì tạo mới
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

        [HttpGet]
        public async Task<IActionResult> Account()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.NguoiDung.FirstOrDefaultAsync(u => u.UserId == userId);
            return View(user);
        }
    }
}