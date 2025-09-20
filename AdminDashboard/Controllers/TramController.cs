using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdminDashboard.Models;
using AdminDashboard.TransportDBContext;

namespace AdminDashboard.Controllers
{
    public class TramController : Controller
    {
        private readonly Db27524Context _context;

        public TramController(Db27524Context context)
        {
            _context = context;
        }

        // GET: Tram
        public async Task<IActionResult> Index()
        {
            return View(await _context.Tram.ToListAsync());
        }

        // GET: Tram/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var tram = await _context.Tram.FirstOrDefaultAsync(m => m.IdTram == id);
            if (tram == null) return NotFound();

            return View(tram);
        }

        // GET: Tram/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Tram/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenTram,DiaChiTram")] Tram tram)
        {
            // Xóa lỗi của IdTram (vì không nhập từ form) 
            // {[IdTram, SubKey = IdTram, Key = IdTram, ValidationState = Invalid]} Lỗi này
            ModelState.Remove("IdTram");

            if (ModelState.IsValid)
            {
                // Lấy trạm cuối cùng
                var lastTram = await _context.Tram
                    .OrderByDescending(t => t.IdTram)
                    .FirstOrDefaultAsync();

                string newId = "T001";
                if (lastTram != null)
                {
                    int lastNumber = int.Parse(lastTram.IdTram.Substring(1));
                    newId = "T" + (lastNumber + 1).ToString("D3");
                }

                tram.IdTram = newId;

                _context.Add(tram);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(tram);
        }


        // GET: Tram/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var tram = await _context.Tram.FindAsync(id);
            if (tram == null) return NotFound();
            return View(tram);
        }

        // POST: Tram/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("IdTram,TenTram,DiaChiTram")] Tram tram)
        {
            if (id != tram.IdTram) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tram);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Tram.Any(e => e.IdTram == tram.IdTram))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(tram);
        }

        // GET: Tram/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();

            var tram = await _context.Tram.FirstOrDefaultAsync(m => m.IdTram == id);
            if (tram == null) return NotFound();

            return View(tram);
        }

        // POST: Tram/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var tram = await _context.Tram.FindAsync(id);
            if (tram != null)
            {
                _context.Tram.Remove(tram);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
