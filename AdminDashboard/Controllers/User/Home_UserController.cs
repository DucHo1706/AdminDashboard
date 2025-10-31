using AdminDashboard.Models;
using AdminDashboard.Models.TrangThai;
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

        public Home_UserController(Db27524Context context)
        {
            _context = context;
        }

        public IActionResult Home_User()
        {
            // Lấy tất cả các trạm để hiển thị trong dropdown
            var danhSachTram = _context.Tram.ToList();

            // Dùng ViewBag hoặc ViewModel để truyền danh sách này ra View
            ViewBag.DanhSachTram = new SelectList(danhSachTram, "IdTram", "TenTram");

            // Tải các chuyến xe sắp tới để hiển thị trên trang Home User
            var today = DateTime.Today;
            var upcomingTrips = _context.ChuyenXe
                .Include(c => c.LoTrinh)
                    .ThenInclude(lt => lt.TramDiNavigation)
                .Include(c => c.LoTrinh)
                    .ThenInclude(lt => lt.TramToiNavigation)
                .Include(c => c.Xe)
                    .ThenInclude(x => x.LoaiXe)
                .Where(c => c.NgayDi.Date >= today &&
                            c.TrangThai == TrangThaiChuyenXe.DangMoBanVe) 
                .OrderBy(c => c.NgayDi)
                .ThenBy(c => c.GioDi)
                .Take(10)
                .ToList();

            return View(upcomingTrips);
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

            var khachHang = await _context.NguoiDung.FirstOrDefaultAsync(kh => kh.UserId == userId);
            if (khachHang != null)
            {
                khachHang.HoTen = model.HoTen;
                khachHang.Email = model.Email;
                khachHang.SoDienThoai = model.SoDienThoai;
                khachHang.NgaySinh = model.NgaySinh;
            }

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



        public async Task<IActionResult> PurchaseHistory()
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
        // GET: ChuyenXe/DanhSach
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

            // So sánh theo ID Trạm vì dropdown chọn IdTram
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