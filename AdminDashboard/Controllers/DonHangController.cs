using AdminDashboard.Models;
using AdminDashboard.TransportDBContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminDashboard.Controllers
{
    public class DonHangController : Controller
    {
        private readonly Db27524Context _context;
        public DonHangController(Db27524Context context)
        {
            _context = context;
        }
        // Hiển thị danh sách đơn đặt vé
        //public async Task<IActionResult> Index()
        //{
        //    var donHangs = await _context.DonHang
        //        .Include(d => d.KhachHang)
        //        .Include(d => d.LoTrinh)
        //        .ToListAsync();
        //    return View(donHangs);
        //}

        // Hiển thị form đặt vé mới
        public IActionResult Create()
        {
            ViewBag.KhachHangs = _context.NguoiDung.ToList();
            ViewBag.LoTrinhs = _context.LoTrinh.ToList();
            // Nếu cần, load thêm dữ liệu như Xe, LoaiXe, Trạm, v.v.
            return View();
        }

        // Xử lý submit form đặt vé
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DonHang donHang)
        {
            if (ModelState.IsValid)
            {
                _context.DonHang.Add(donHang);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.KhachHangs = _context.NguoiDung.ToList();
            ViewBag.LoTrinhs = _context.LoTrinh.ToList();
            return View(donHang);
        }
    }
}
