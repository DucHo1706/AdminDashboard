using AdminDashboard.Models;
using AdminDashboard.TransportDBContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminDashboard.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class LoaiXeController : Controller
    {
        private readonly Db27524Context _context;
        public LoaiXeController(Db27524Context context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _context.LoaiXe.ToListAsync());
        }
        // GET: LoaiXe/Create
        public IActionResult Create()
        {
            return View();
        }
        // POST: LoaiXe/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenLoaiXe")] LoaiXe loaixe)
        {
        
            ModelState.Remove("LoaiXeId");

            if (ModelState.IsValid)
            {
                loaixe.LoaiXeId = Guid.NewGuid().ToString("N").Substring(0, 8);
                _context.Add(loaixe);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(loaixe);
        }
        // GET: LoaiXe/Details/
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loaixe = await _context.LoaiXe
                .FirstOrDefaultAsync(m => m.LoaiXeId == id);

            if (loaixe == null)
            {
                return NotFound();
            }

            return View(loaixe);
        }

        // GET: LoaiXe/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loaixe = await _context.LoaiXe.FindAsync(id);
            if (loaixe == null)
            {
                return NotFound();
            }
            return View(loaixe);
        }

        // POST: LoaiXe/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("LoaiXeId,TenLoaiXe")] LoaiXe loaixe)
        {
            if (id != loaixe.LoaiXeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(loaixe);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LoaiXeExists(loaixe.LoaiXeId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(loaixe);
        }
        // GET: LoaiXe/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loaixe = await _context.LoaiXe
                .FirstOrDefaultAsync(m => m.LoaiXeId == id);

            if (loaixe == null)
            {
                return NotFound();
            }

            return View(loaixe);
        }

        // POST: LoaiXe/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var loaixe = await _context.LoaiXe.FindAsync(id);
            if (loaixe != null)
            {
                _context.LoaiXe.Remove(loaixe);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LoaiXeExists(string id)
        {
            return _context.LoaiXe.Any(e => e.LoaiXeId == id);
        }

    }
}
