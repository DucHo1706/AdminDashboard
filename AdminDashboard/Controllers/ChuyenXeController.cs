using AdminDashboard.Models;
using AdminDashboard.TransportDBContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AdminDashboard.Controllers
{
	public class ChuyenXeController : Controller
	{
		private readonly Db27524Context _context; // Thay Db27524Context bằng tên DbContext của bạn nếu khác

		public ChuyenXeController(Db27524Context context)
		{
			_context = context;
		}

		// GET: ChuyenXe
		public async Task<IActionResult> Index()
		{
			// Dùng Include để lấy thông tin của Lộ trình và Xe liên quan
			var chuyenXes = _context.ChuyenXe
				.Include(c => c.LoTrinh)
				.Include(c => c.Xe);
			return View(await chuyenXes.ToListAsync());
		}

		// GET: ChuyenXe/Details/5
		public async Task<IActionResult> Details(string id)
		{
			if (id == null) return NotFound();

			var chuyenXe = await _context.ChuyenXe
				.Include(c => c.LoTrinh)
				.Include(c => c.Xe)
				.FirstOrDefaultAsync(m => m.ChuyenId == id);

			if (chuyenXe == null) return NotFound();

			return View(chuyenXe);
		}

        // GET: ChuyenXe/Create
        public IActionResult Create()
        {
            ViewBag.LoTrinhId = new SelectList(_context.LoTrinh, "Id", "TenLoTrinh");
            ViewBag.XeId = new SelectList(_context.Xe, "Id", "BienSoXe");
            return View();
        }

        // POST: ChuyenXe/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("...")] ChuyenXe chuyenXe)
        {
            if (ModelState.IsValid)
            {
                _context.Add(chuyenXe);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.LoTrinhId = new SelectList(_context.LoTrinh, "LoTrinhId", "TenLoTrinh", chuyenXe.LoTrinhId);
            ViewBag.XeId = new SelectList(_context.Xe, "XeId", "BienSo", chuyenXe.XeId);

            return View(chuyenXe);
        }


        // GET: ChuyenXe/Edit/5
        public async Task<IActionResult> Edit(string id)
		{
			if (id == null) return NotFound();

			var chuyenXe = await _context.ChuyenXe.FindAsync(id);
			if (chuyenXe == null) return NotFound();

			PopulateDropdownLists(chuyenXe.LoTrinhId, chuyenXe.XeId);
			return View(chuyenXe);
		}

		// POST: ChuyenXe/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(string id, [Bind("ChuyenId,LoTrinhId,XeId,NgayDi,GioDi,GioDenDuKien,TrangThai")] ChuyenXe chuyenXe)
		{
			if (id != chuyenXe.ChuyenId) return NotFound();

			ModelState.Remove("LoTrinh");
			ModelState.Remove("Xe");

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(chuyenXe);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!_context.ChuyenXe.Any(e => e.ChuyenId == chuyenXe.ChuyenId))
						return NotFound();
					else
						throw;
				}
				return RedirectToAction(nameof(Index));
			}

			PopulateDropdownLists(chuyenXe.LoTrinhId, chuyenXe.XeId);
			return View(chuyenXe);
		}

		// GET: ChuyenXe/Delete/5
		public async Task<IActionResult> Delete(string id)
		{
			if (id == null) return NotFound();

			var chuyenXe = await _context.ChuyenXe
				.Include(c => c.LoTrinh)
				.Include(c => c.Xe)
				.FirstOrDefaultAsync(m => m.ChuyenId == id);

			if (chuyenXe == null) return NotFound();

			return View(chuyenXe);
		}

		// POST: ChuyenXe/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(string id)
		{
			var chuyenXe = await _context.ChuyenXe.FindAsync(id);
			if (chuyenXe != null)
			{
				_context.ChuyenXe.Remove(chuyenXe);
				await _context.SaveChangesAsync();
			}
			return RedirectToAction(nameof(Index));
		}

		// Hàm hỗ trợ để lấy dữ liệu cho dropdown, tránh lặp code
		private void PopulateDropdownLists(object selectedLoTrinh = null, object selectedXe = null)
		{
			// Giả định Model LoTrinh có thuộc tính TenLoTrinh và Xe có thuộc tính BienSoXe để hiển thị
			ViewData["LoTrinhId"] = new SelectList(_context.Set<LoTrinh>(), "LoTrinhId", "TenLoTrinh", selectedLoTrinh);
			ViewData["XeId"] = new SelectList(_context.Set<Xe>(), "XeId", "BienSoXe", selectedXe);
		}
	}
}
