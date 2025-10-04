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
        //ACCOUNT
        public async Task<IActionResult> Account()
        {
            // Get the current user's UserId from claims
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                // If user is not authenticated, redirect to login
                return RedirectToAction("Login", "Auth");
            }

            // Fetch the user from the database
            var user = await _context.NguoiDung
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                // Handle case where user is not found
                return View(new NguoiDung()); // Pass an empty model to avoid null reference in view
            }

            // Fetch the user's role
            var role = await (from ur in _context.UserRole
                              join r in _context.VaiTro on ur.RoleId equals r.RoleId
                              where ur.UserId == user.UserId
                              select r.TenVaiTro).FirstOrDefaultAsync() ?? "Khach";

            // Pass the role to the view via ViewData
            ViewData["VaiTro"] = role;

            // Pass the user model to the view
            return View(user);
        }


        //EditAccount
        //EditAccount
        // GET: EditAccount
        [HttpGet]
        public async Task<IActionResult> EditAccount()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _context.NguoiDung
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: EditAccount
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAccount(NguoiDung model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _context.NguoiDung
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Cập nhật thông tin User
                user.HoTen = model.HoTen;
                user.Email = model.Email;
                user.SoDienThoai = model.SoDienThoai;
                user.NgaySinh = model.NgaySinh;

                // Nếu có KhachHang mapping thì update luôn
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
                    TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                    return RedirectToAction("Account");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi khi lưu thông tin: {ex.Message}");
                }
            }

            return View(model);
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
                              select r.TenVaiTro).FirstOrDefaultAsync() ?? "Khach";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId),
                new Claim(ClaimTypes.Name, user.TenDangNhap),
                new Claim(ClaimTypes.Role, role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync("CookieAuth", claimsPrincipal);

            // Chuyển hướng dựa trên vai trò
            if (role == "Admin")
            {
                return RedirectToAction("Index", "Home");
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

            // Kiểm tra tên đăng nhập hoặc email đã tồn tại
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

            // Tạo UserId mới
            string newUserId;
            do
            {
                newUserId = "U" + Guid.NewGuid().ToString("N").Substring(0, 5);
            } while (await _context.NguoiDung.AnyAsync(u => u.UserId == newUserId));

            var user = new NguoiDung
            {
                UserId = newUserId,
                TenDangNhap = model.TenDangNhap,
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
                new Claim(ClaimTypes.Name, user.TenDangNhap),
                new Claim(ClaimTypes.Role, "KhachHang")
            };

            var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync("CookieAuth", claimsPrincipal);

            // Chuyển hướng đến trang thông tin tài khoản
            return RedirectToAction("Account", "Auth");
        }
    }
}