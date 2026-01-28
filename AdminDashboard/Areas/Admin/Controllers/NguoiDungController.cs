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
    //[Authorize(Roles = "Admin")]
    public class NguoiDungController : Controller
    {
        // Khai báo field private như bình thường, không dùng attributes
        private readonly Db27524Context _context;

        public NguoiDungController(Db27524Context context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 5;

            // Đếm tổng số bản ghi trong bảng Người dùng
            var totalRecords = await _context.NguoiDung.CountAsync();

            // Lấy danh sách người dùng cho trang hiện tại
            var nguoiDungs = await _context.NguoiDung
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.VaiTro)
                .OrderBy(u => u.HoTen)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Tính tổng số trang
            ViewBag.TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            ViewBag.CurrentPage = page;

            return View(nguoiDungs);
        }

    }
}