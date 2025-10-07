using AdminDashboard.Models;
using AdminDashboard.TransportDBContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminDashboard.Controllers
{
    public class KhachHangController : Controller
    {
        private readonly Db27524Context _context;
        public KhachHangController(Db27524Context context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _context.Tram.ToListAsync());
        }
        // Details
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();
            var kh = await _context.KhachHang.FirstOrDefaultAsync(m => m.IDKhachHang == id);
            if (kh == null) return NotFound();
            return View(kh);
        }
        public IActionResult Create()
        {
            return View();
        }

        // Create POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenKhachHang,SoDienThoai,DiaChi")] KhachHang kh)
        {
            if (ModelState.IsValid)
            {
                var last = await _context.KhachHang.OrderByDescending(k => k.IDKhachHang).FirstOrDefaultAsync();
                string newId = "KH001";
                if (last != null)
                {
                    int num = int.Parse(last.IDKhachHang.Substring(2));
                    newId = "KH" + (num + 1).ToString("D3");
                }
                kh.IDKhachHang = newId;

                _context.Add(kh);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(kh);
        }

        // Edit GET
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();
            var kh = await _context.KhachHang.FindAsync(id);
            if (kh == null) return NotFound();
            return View(kh);
        }

        // Edit POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("IdKhachHang,TenKhachHang,SoDienThoai,DiaChi")] KhachHang kh)
        {
            if (id != kh.IDKhachHang) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(kh);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(kh);
        }

        // Delete GET
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();
            var kh = await _context.KhachHang.FirstOrDefaultAsync(m => m.IDKhachHang == id);
            if (kh == null) return NotFound();
            return View(kh);
        }

        // Delete POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var kh = await _context.KhachHang.FindAsync(id);
            if (kh != null)
            {
                _context.KhachHang.Remove(kh);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

    }
}
