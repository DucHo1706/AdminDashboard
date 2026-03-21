using Microsoft.AspNetCore.Mvc;
using AdminDashboard.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using AdminDashboard.TransportDBContext;

namespace AdminDashboard.ViewComponents
{
    public class TimKiemXeViewComponent : ViewComponent
    {
        private readonly Db27524Context _context;

        public TimKiemXeViewComponent(Db27524Context context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke(string diemDi = null, string diemDen = null)
        {
            // Lấy danh sách trạm từ database và chuyển sang SelectListItem
            var tramList = _context.Tram
                .Select(t => new SelectListItem
                {
                    Value = t.IdTram,
                    Text = t.TenTram
                })
                .ToList();

            // Truyền vào ViewBag để dùng cho dropdown
            ViewBag.TramDiList = tramList;
            ViewBag.TramDenList = tramList;

            ViewBag.DiemDi = diemDi;
            ViewBag.DiemDen = diemDen;

            return View();
        }
    }
}
