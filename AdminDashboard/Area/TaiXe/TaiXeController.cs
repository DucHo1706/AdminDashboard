using AdminDashboard.TransportDBContext;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace AdminDashboard.Area.TaiXe
{
    public class TaiXeController : Controller
    {
        private readonly Db27524Context _context;
        public TaiXeController(Db27524Context context)
        {
            _context = context;

        }
        // GET: /TaiXe/LichLamViec
        public async Task<IActionResult> LichLamViec()
        {
            // Lấy ID của tài xế đang đăng nhập
            var idTaiXe = User.FindFirstValue(ClaimTypes.NameIdentifier);

            //if (string.IsNullOrEmpty(idTaiXe))
            //{
            //    return Unauthorized(); // Không tìm thấy thông tin đăng nhập
            //}

            // Lấy danh sách các chuyến xe được phân công cho tài xế này
            // Chỉ lấy các chuyến từ hôm nay trở về sau
            var lichCuaToi = await _context.ChuyenXe
                .Where(c => c.TaiXeId == idTaiXe && c.NgayDi >= DateTime.Today)
                .Include(c => c.LoTrinh.TramDiNavigation)
                .Include(c => c.LoTrinh.TramToiNavigation)
                .Include(c => c.Xe)
                .OrderBy(c => c.NgayDi).ThenBy(c => c.GioDi) // Sắp xếp theo thời gian
                .ToListAsync();

            return View(lichCuaToi);
        }
    }
}
