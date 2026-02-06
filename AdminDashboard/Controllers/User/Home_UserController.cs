using AdminDashboard.Facades;
using AdminDashboard.Models;
using AdminDashboard.Models.TrangThai;
using AdminDashboard.Models.ViewModels;
using AdminDashboard.TransportDBContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AdminDashboard.Controllers
{
    public class Home_UserController : Controller
    {
        private readonly Db27524Context _context;
        private readonly IHomeFacade _homeFacade; 

        public Home_UserController(Db27524Context context, IHomeFacade homeFacade)
        {
            _context = context;
            _homeFacade = homeFacade;
        }

        public IActionResult Home_User()
        {
            var danhSachTram = _context.Tram.ToList();
            ViewBag.DanhSachTram = new SelectList(danhSachTram, "IdTram", "TenTram");

            // 2. Sử dụng Facade để lấy dữ liệu phức tạp
            ViewBag.RoutesByDeparture = _homeFacade.LayTuyenXeNoiBat();

            // 3. Lấy chuyến xe hôm nay qua Facade
            var tripsToday = _homeFacade.LayChuyenXeHomNay();

            return View(tripsToday);
        }

        public async Task<IActionResult> Account()
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
                return NotFound("Không tìm thấy thông tin người dùng.");
            }

            var role = await (from ur in _context.UserRole
                              join r in _context.VaiTro on ur.RoleId equals r.RoleId
                              where ur.UserId == userId
                              select r.TenVaiTro).FirstOrDefaultAsync() ?? "Khach";

            ViewData["VaiTro"] = role;

            return View(user);
        }

        public async Task<IActionResult> EditAccount()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _context.NguoiDung.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return NotFound("Không tìm thấy thông tin người dùng.");
            }

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> EditAccount(NguoiDung model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _context.NguoiDung.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return NotFound("Không tìm thấy thông tin người dùng.");
            }

            user.HoTen = model.HoTen;
            user.Email = model.Email;
            user.SoDienThoai = model.SoDienThoai;
            user.NgaySinh = model.NgaySinh;

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction("Account");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi khi lưu thông tin: {ex.Message}");
                return View(model);
            }
        }

        public IActionResult PurchaseHistory()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var viewModel = _homeFacade.LayLichSuDonHang(userId);

            return View(viewModel);
        }

        public IActionResult ChuyenXe_User()
        {
            ViewBag.DanhSachTram = new SelectList(_context.Tram, "IdTram", "TenTram");
            var danhSach = _context.ChuyenXe
                .Include(c => c.LoTrinh).ThenInclude(l => l.TramDiNavigation)
                .Include(c => c.LoTrinh).ThenInclude(l => l.TramToiNavigation)
                .Include(c => c.Xe)
                .Where(c => c.TrangThai == TrangThaiChuyenXe.DangMoBanVe
                         || c.TrangThai == TrangThaiChuyenXe.ChoKhoiHanh
                         || c.TrangThai == TrangThaiChuyenXe.DaLenLich)
                .ToList();

            return View(danhSach);
        }

        [HttpGet]
        public IActionResult TimKiemAjax(string diemDi, string diemDen, string ngayDi)
        {
            var query = _context.ChuyenXe
                .Include(c => c.LoTrinh).ThenInclude(l => l.TramDiNavigation)
                .Include(c => c.LoTrinh).ThenInclude(l => l.TramToiNavigation)
                .Include(c => c.Xe)
                .Where(c => c.TrangThai == TrangThaiChuyenXe.DangMoBanVe
                         || c.TrangThai == TrangThaiChuyenXe.ChoKhoiHanh
                         || c.TrangThai == TrangThaiChuyenXe.DaLenLich)
                .AsQueryable();

            if (!string.IsNullOrEmpty(diemDi))
                query = query.Where(c => c.LoTrinh.TramDi == diemDi);

            if (!string.IsNullOrEmpty(diemDen))
                query = query.Where(c => c.LoTrinh.TramToi == diemDen);

            if (!string.IsNullOrEmpty(ngayDi) && DateTime.TryParse(ngayDi, out DateTime parsedNgay))
                query = query.Where(c => c.NgayDi.Date == parsedNgay.Date);

            var ketQua = query
                .OrderBy(c => c.NgayDi)
                .ThenBy(c => c.GioDi)
                .ToList();

            return PartialView("_DanhSachChuyenXe", ketQua);
        }

        public IActionResult About()
        {
            return View();
        }
    }
}