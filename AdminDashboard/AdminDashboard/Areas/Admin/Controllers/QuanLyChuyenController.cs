using AdminDashboard.Models.TrangThai;
using AdminDashboard.Services;
using AdminDashboard.TransportDBContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminDashboard.Areas.Admin.Controllers
{
    [Area("Admin")]
    // [Authorize(Roles = "Admin")] // Nhớ bật lại sau
    public class QuanLyChuyenController : Controller
    {
        private readonly Db27524Context _context; // Vẫn cần context để load danh sách (Read)
        private readonly IChuyenXeService _chuyenXeService; // Dùng service cho các hành động (Write)

        public QuanLyChuyenController(Db27524Context context, IChuyenXeService chuyenXeService)
        {
            _context = context;
            _chuyenXeService = chuyenXeService;
        }

        // 1. DANH SÁCH CHỜ DUYỆT (Giữ nguyên logic hiển thị)
        public async Task<IActionResult> Index(int? trangThai)
        {
            int statusFilter = trangThai ?? (int)TrangThaiChuyenXe.ChoDuyet;

            var query = _context.ChuyenXe
                .Include(c => c.LoTrinh).ThenInclude(lt => lt.TramDiNavigation)
                .Include(c => c.LoTrinh).ThenInclude(lt => lt.TramToiNavigation)
                .Include(c => c.Xe).ThenInclude(x => x.NhaXe)
                .AsQueryable();

            if (statusFilter != -99)
            {
                query = query.Where(c => (int)c.TrangThai == statusFilter);
            }

            // Sắp xếp: Ưu tiên ngày gần nhất để duyệt gấp
            var result = await query.OrderBy(c => c.NgayDi).ThenBy(c => c.GioDi).ToListAsync();

            ViewBag.CurrentStatus = statusFilter;
            return View(result);
        }

        // 2. DUYỆT HÀNG LOẠT (Gọi Service)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DuyetNhanh(List<string> selectedIds)
        {
            if (selectedIds == null || !selectedIds.Any())
            {
                TempData["ErrorMessage"] = "Chưa chọn chuyến nào.";
                return RedirectToAction(nameof(Index));
            }

            // Gọi Service xử lý
            int count = await _chuyenXeService.DuyetNhieuChuyenAsync(selectedIds, "ADMIN_ID_HIEN_TAI");

            if (count > 0)
                TempData["SuccessMessage"] = $"Đã duyệt thành công {count} chuyến xe.";
            else
                TempData["ErrorMessage"] = "Không có chuyến nào hợp lệ để duyệt (Có thể đã được duyệt trước đó).";

            return RedirectToAction(nameof(Index));
        }
    }
}