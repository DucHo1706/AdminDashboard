using AdminDashboard.Models;
using AdminDashboard.TransportDBContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminDashboard.Controllers
{
    public class XeController : Controller
    {
        private readonly Db27524Context _context;
        public XeController(Db27524Context context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _context.Xe.ToListAsync());
        }
        // Details
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();
            var xe = await _context.Xe.FirstOrDefaultAsync(m => m.LoaiXeId == id);
            if (xe == null) return NotFound();
            return View(xe);
        }
        public IActionResult Create()
        {
            return View();
        }
        // Create POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LoaiXeId,TenLoaiXe,SoChoNgoi")] Xe xe)
        {
            if (ModelState.IsValid)
            {
                var last = await _context.Xe.OrderByDescending(k => k.LoaiXeId).FirstOrDefaultAsync();
                string newId = "LX001";
                if (last != null)
                {
                    int num = int.Parse(last.LoaiXeId.Substring(2));
                    newId = "LX" + (num + 1).ToString("D3");
                }
                xe.LoaiXeId = newId;

                _context.Add(xe);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(xe);
        }
        // Edit GET
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("LoaiXeId,TenLoaiXe,SoChoNgoi")] Xe xe)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(xe);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!XeExists(xe.LoaiXeId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(xe);
        }
        private bool XeExists(string id)
        {
            return _context.Xe.Any(e => e.LoaiXeId == id);
        }
        // Delete GET
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();
            var kh = await _context.Xe.FirstOrDefaultAsync(m => m.LoaiXeId == id);
            if (kh == null) return NotFound();
            return View(kh);
        }

        // Delete POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var kh = await _context.Xe.FindAsync(id);
            if (kh != null)
            {
                _context.Xe.Remove(kh);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }



    }

}
