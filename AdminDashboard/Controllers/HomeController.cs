
using AdminDashboard.Models;
using AdminDashboard.TransportDBContext;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace AdminDashboard.Controllers
{
    public class HomeController : Controller
    {
        private readonly Db27524Context _context;

        public HomeController(Db27524Context context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Dashboard");
        }

        public IActionResult Dashboard()
        {
            var tongKhachHang = _context.NguoiDung.Count();
            var tongDonHang = _context.DonHang.Count();
            var danhSachKhachHang = _context.NguoiDung.ToList();

            // Tính doanh thu hôm nay dựa trên TongTien
            var doanhThuHomNay = _context.DonHang
                .Where(d => d.NgayDat.Date == DateTime.Today)
                .Sum(d => (decimal?)d.TongTien) ?? 0;

            ViewBag.TongKhachHang = tongKhachHang;
            ViewBag.TongDonHang = tongDonHang;
            ViewBag.DoanhThuHomNay = doanhThuHomNay;
            ViewBag.DanhSachKhachHang = danhSachKhachHang;

            return View();
        }

    }
}
