using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdminDashboard.Models;
using AdminDashboard.TransportDBContext;
using Microsoft.AspNetCore.Authorization;

namespace AdminDashboard.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TramController : Controller
    {
        private readonly Db27524Context _context;

        public TramController(Db27524Context context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Tram.ToListAsync());
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var tram = await _context.Tram.FirstOrDefaultAsync(m => m.IdTram == id);
            if (tram == null) return NotFound();

            return View(tram);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenTram,DiaChiTram,Tinh,Huyen,Xa")] Tram tram)
        {
            ModelState.Remove("IdTram");

            if (ModelState.IsValid)
            {
                string newId;
                do
                {
                    newId = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
                } while (await _context.Tram.AnyAsync(t => t.IdTram == newId));

                tram.IdTram = newId;

                _context.Add(tram);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(tram);
        }


        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var tram = await _context.Tram.FindAsync(id);
            if (tram == null) return NotFound();
            return View(tram);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("IdTram,TenTram,DiaChiTram,Tinh,Huyen,Xa")] Tram tram)
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

        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();

            var tram = await _context.Tram.FirstOrDefaultAsync(m => m.IdTram == id);
            if (tram == null) return NotFound();

            return View(tram);
        }

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