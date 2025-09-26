using AdminDashboard.Models;
using AdminDashboard.TransportDBContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AdminDashboard.Area.Admin.Controllers
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
			var dsXe = _context.Xe.Include(x => x.LoaiXe);
			return View(await dsXe.ToListAsync());
		}
        // GET: Xe/Create
        public IActionResult Create()
        {
            ViewData["LoaiXeId"] = new SelectList(_context.LoaiXe, "LoaiXeId", "TenLoaiXe");
            return View();
        }
        // POST: Xe/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BienSoXe,LoaiXeId")] Xe xe)
        {
            ModelState.Remove("XeId");

            if (ModelState.IsValid)
            {
                xe.XeId = Guid.NewGuid().ToString("N").Substring(0, 8); // tự sinh ID
                try
                {
                    _context.Add(xe);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi lưu dữ liệu: " + ex.Message);
                }

            }

            ViewData["LoaiXeId"] = new SelectList(_context.LoaiXe, "LoaiXeId", "TenLoaiXe", xe.LoaiXeId);
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                              .Select(e => e.ErrorMessage)
                              .ToList();
            Console.WriteLine("Errors: " + string.Join(" | ", errors));

            return View(xe);
        }

        // GET: Xe/Details/
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var xe = await _context.Xe
                                   .Include(x => x.LoaiXe)  
                                   .FirstOrDefaultAsync(m => m.XeId == id);

            if (xe == null)
            {
                return NotFound();
            }

            return View(xe);
        }

        // GET: Xe/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var xe = await _context.Xe.FindAsync(id);
            if (xe == null)
            {
                return NotFound();
            }

            ViewData["LoaiXeId"] = new SelectList(_context.LoaiXe, "LoaiXeId", "TenLoaiXe", xe.LoaiXeId);
            return View(xe);
        }

        // POST: Xe/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("XeId,BienSoXe,LoaiXeId")] Xe xe)
        {
            if (id != xe.XeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(xe);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Xe.Any(e => e.XeId == xe.XeId))
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
            ViewData["LoaiXeId"] = new SelectList(_context.LoaiXe, "LoaiXeId", "TenLoaiXe", xe.LoaiXeId);
            return View(xe);
        }

        // GET: Xe/Delete/
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }   

            var xe = await _context.Xe
                .FirstOrDefaultAsync(m => m.XeId == id);

            if (xe == null)
            {
                return NotFound();
            }

            return View(xe);
        }

        // POST: Xe/Delete/
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var xe = await _context.Xe.FindAsync(id);
            if (xe != null)
            {
                _context.Xe.Remove(xe);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LoaiXeExists(string id)
        {
            return _context.Xe.Any(e => e.XeId == id);
        }

    }
}
