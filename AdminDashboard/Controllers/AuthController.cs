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

            var user = _context.NguoiDung
                .FirstOrDefault(u => u.TenDangNhap == model.TenDangNhap
                                  && u.MatKhau == model.MatKhau); // tạm thời để plain text

            if (user == null)
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng");
                return View(model);
            }
            var role = await(from ur in _context.UserRole
                             join r in _context.VaiTro on ur.RoleId equals r.RoleId
                             where ur.UserId == user.UserId
                             select r.TenVaiTro).FirstOrDefaultAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.TenDangNhap),
                new Claim(ClaimTypes.Role, role ?? "Khach")
            };

            var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync("CookieAuth", claimsPrincipal);


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
            if (!ModelState.IsValid)
                return View(model);

            // Kiểm tra tên đăng nhập hoặc email đã tồn tại
            if (_context.NguoiDung.Any(u => u.TenDangNhap == model.TenDangNhap || u.Email == model.Email))
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc Email đã tồn tại");
                return View(model);
            }

            // Tạo UserId mới
            string newUserId = "U" + Guid.NewGuid().ToString("N").Substring(0, 5);

            var user = new NguoiDung
            {
                UserId = newUserId,
                TenDangNhap = model.TenDangNhap,
                MatKhau = model.MatKhau, // TODO: Hash mật khẩu sau
                Email = model.Email,
                HoTen = model.HoTen,
                SoDienThoai = model.SoDienThoai,
                NgaySinh = model.NgaySinh,
                TrangThai = "Hoạt động"
            };

            _context.NguoiDung.Add(user);

            // Gán role mặc định là "KhachHang"
            var roleId = _context.VaiTro.FirstOrDefault(r => r.TenVaiTro == "KhachHang")?.RoleId;
            if (roleId != null)
            {
                _context.UserRole.Add(new UserRole
                {
                    UserId = newUserId,
                    RoleId = roleId
                });
            }

            // Đồng thời tạo KhachHang mapping
            string newKhId = "KH" + Guid.NewGuid().ToString("N").Substring(0, 5);
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

            // Sau khi đăng ký thì tự động login
            return RedirectToAction("Login", "Auth");
        }


    }
}

