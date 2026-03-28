using AdminDashboard.Facades;
using AdminDashboard.Models;
using AdminDashboard.Models.TrangThai;
using AdminDashboard.Models.ViewModels;
using AdminDashboard.TransportDBContext;
using AdminDashboard.Patterns.Command;
using AdminDashboard.Patterns.Strategy;
using AdminDashboard.Patterns.Observer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using AdminDashboard.Patterns.ChainOfResponsibility;

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

            ViewBag.RoutesByDeparture = _homeFacade.LayTuyenXeNoiBat();
            var tripsToday = _homeFacade.LayChuyenXeHomNay();

            return View(tripsToday);
        }

        public async Task<IActionResult> Account()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Auth");

            var user = await _context.NguoiDung
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return NotFound("Không tìm thấy thông tin người dùng.");

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
                return RedirectToAction("Login", "Auth");

            var user = await _context.NguoiDung.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return NotFound("Không tìm thấy thông tin người dùng.");

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> EditAccount(NguoiDung model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Auth");

            var user = await _context.NguoiDung.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return NotFound("Không tìm thấy thông tin người dùng.");

            try
            {
                // ✅ COMMAND
                ICommand cmd = new UpdateUserCommand(_context, user, model);
                cmd.Execute();

                // ✅ OBSERVER
                DashboardService service = new DashboardService();
                service.Notify("User updated");

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
                return RedirectToAction("Login", "Auth");

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

            // 1. Gói dữ liệu vào Request
            var request = new TimKiemRequest
            {
                Query = query,
                DiemDi = diemDi,
                DiemDen = diemDen,
                NgayDi = ngayDi
            };

            // 2. Khởi tạo các bộ lọc (Các mắt xích)
            var locDiemDi = new LocTheoDiemDiHandler();
            var locDiemDen = new LocTheoDiemDenHandler();
            var locNgayDi = new LocTheoNgayDiHandler();

            // 3. Móc nối các bộ lọc lại thành 1 chuỗi và đưa Request vào xử lý
            locDiemDi.SetNext(locDiemDen).SetNext(locNgayDi);
            var querySauKhiLoc = locDiemDi.Handle(request);


            ISortStrategy strategy = new SortByDateStrategy();

            var ketQua = strategy.Sort(querySauKhiLoc.ToList());

            return PartialView("_DanhSachChuyenXe", ketQua);
        }

        public IActionResult About()
        {
            return View();
        }
    }
}