using AdminDashboard.Models;
using AdminDashboard.TransportDBContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace AdminDashboard.Areas.Admin.Controllers
{
    // Áp dụng Area và Authorization cho toàn bộ Controller
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class NguoiDungController : Controller
    {
        // Khai báo field private như bình thường, không dùng attributes
        private readonly Db27524Context _context;

        public NguoiDungController(Db27524Context context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Logic tải dữ liệu: Đã đúng và tận dụng Navigation Property 'VaiTro'
            var nguoiDungs = await _context.NguoiDung
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.VaiTro)
                .OrderBy(u => u.HoTen)
                .ToListAsync();

            // Trả về View và truyền danh sách vào
            return View(nguoiDungs);
        }

        // Bạn có thể thêm các action khác như Create, Edit, Delete vào đây
    }
}