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

        public Home_UserController(Db27524Context context)
        {
            _context = context;
        }

        public IActionResult Home_User()
        {
            // 1. Dropdown trạm (Giữ nguyên)
            var danhSachTram = _context.Tram.ToList();
            ViewBag.DanhSachTram = new SelectList(danhSachTram, "IdTram", "TenTram");

            var today = DateTime.Today; // Lấy ngày hôm nay (00:00:00)

            // ---------------------------------------------------------------------
            // PHẦN 1: LẤY DỮ LIỆU ĐỂ TẠO "TUYẾN XE NỔI BẬT" (ViewBag)
            // (Vẫn lấy các chuyến >= hôm nay để gom nhóm hiển thị cho đẹp)
            // ---------------------------------------------------------------------
            var allUpcomingTrips = _context.ChuyenXe
                .Include(c => c.LoTrinh).ThenInclude(lt => lt.TramDiNavigation)
                .Include(c => c.LoTrinh).ThenInclude(lt => lt.TramToiNavigation)
                .Include(c => c.Images)
                .Where(c => c.NgayDi.Date >= today &&
                            c.TrangThai == TrangThaiChuyenXe.DangMoBanVe)
                .ToList();

            // Logic gom nhóm Tuyến xe (Giữ nguyên logic của bạn)
            var routesByDeparture = allUpcomingTrips
                .Where(c => c.LoTrinh?.TramDiNavigation != null)
                .GroupBy(c => new {
                    TenTram = c.LoTrinh.TramDiNavigation.TenTram,
                    Tinh = c.LoTrinh.TramDiNavigation.Tinh ?? "",
                    ImageUrl = c.Images?.FirstOrDefault()?.ImageUrl ?? "/images/slider/hcm.png"
                })
                .Select(g => new TuyenXeViewModel
                {
                    Tinh = g.Key.Tinh,
                    TenTram = g.Key.TenTram,
                    ImageUrl = g.Key.ImageUrl,
                    TuyenXe = g.Where(c => c.LoTrinh?.TramToiNavigation != null)
                        .GroupBy(c => c.LoTrinh.TramToiNavigation.TenTram)
                        .Select(group => group.OrderBy(c => c.NgayDi).ThenBy(c => c.GioDi).First())
                        .Select(c => new TuyenXeItemViewModel
                        {
                            ChuyenId = c.ChuyenId,
                            DiemDen = c.LoTrinh.TramToiNavigation.TenTram,
                            NgayDi = c.NgayDi,
                            GioDi = c.GioDi,
                            GioDenDuKien = c.GioDenDuKien,
                            ThoiGian = (c.GioDenDuKien - c.GioDi).TotalHours >= 1
                                ? $"{(int)(c.GioDenDuKien - c.GioDi).TotalHours} giờ"
                                : $"{(int)((c.GioDenDuKien - c.GioDi).TotalMinutes)} phút",
                            GiaVe = c.LoTrinh.GiaVeCoDinh ?? 0,
                            ImageUrl = c.Images?.FirstOrDefault()?.ImageUrl ?? g.Key.ImageUrl
                        })
                        .OrderBy(t => t.DiemDen)
                        .Take(3)
                        .ToList()
                })
                .Where(r => r.TuyenXe != null && r.TuyenXe.Any())
                .Take(3)
                .ToList();

            ViewBag.RoutesByDeparture = routesByDeparture;

            // ---------------------------------------------------------------------
            // PHẦN 2: LẤY DỮ LIỆU "CHUYẾN XE HÔM NAY" (Model chính trả về View)
            // (Chỉ lấy chính xác ngày hôm nay để hiển thị list bên dưới)
            // ---------------------------------------------------------------------
            var tripsToday = _context.ChuyenXe
                .Include(c => c.LoTrinh).ThenInclude(lt => lt.TramDiNavigation)
                .Include(c => c.LoTrinh).ThenInclude(lt => lt.TramToiNavigation)
                .Include(c => c.Xe).ThenInclude(x => x.LoaiXe) // Include loại xe để hiển thị
                .Include(c => c.Images)
                .Where(c => c.NgayDi.Date == today && // <--- QUAN TRỌNG: Chỉ lấy ngày hôm nay
                            c.TrangThai == TrangThaiChuyenXe.DangMoBanVe)
                .OrderBy(c => c.GioDi) // Sắp xếp theo giờ chạy
                .ToList();

            // Trả về danh sách chuyến hôm nay cho Model
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

        public async Task<IActionResult> PurchaseHistory()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var donHangs = await _context.DonHang
                .Where(d => d.IDKhachHang == userId)
                .Include(d => d.ChuyenXe).ThenInclude(cx => cx.LoTrinh).ThenInclude(lt => lt.TramDiNavigation)
                .Include(d => d.ChuyenXe).ThenInclude(cx => cx.LoTrinh).ThenInclude(lt => lt.TramToiNavigation)
                .OrderByDescending(d => d.NgayDat)
                .ToListAsync();

            return View(donHangs);
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