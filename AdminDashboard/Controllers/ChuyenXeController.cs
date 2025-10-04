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
            var chuyenXes = await _context.ChuyenXe
         .Include(c => c.LoTrinh)
             .ThenInclude(lt => lt.TramDiNavigation)
         .Include(c => c.LoTrinh)
             .ThenInclude(lt => lt.TramToiNavigation)
         .Include(c => c.Xe)
         .ToListAsync();

            return View(chuyenXes);
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
			PopulateDropdownLists();
            return View();
		}

		// POST: ChuyenXe/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("LoTrinhId,XeId,NgayDi,GioDi,GioDenDuKien,TrangThai")] ChuyenXe chuyenXe)
		{
			// Bỏ qua validation cho các thuộc tính không được binding từ form
			ModelState.Remove("ChuyenId");
			ModelState.Remove("LoTrinh");
			ModelState.Remove("Xe");

            if (ModelState.IsValid)
            {
                chuyenXe.ChuyenId = Guid.NewGuid().ToString("N").Substring(0, 8);
                _context.Add(chuyenXe);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã tạo chuyến xe thành công.";
                return RedirectToAction(nameof(Index));
            }

            // Khi ModelState không hợp lệ, cần nạp lại dropdowns
            PopulateDropdownLists(chuyenXe.LoTrinhId, chuyenXe.XeId);

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
            var loTrinhs = _context.LoTrinh
                .Include(lt => lt.TramDiNavigation)
                .Include(lt => lt.TramToiNavigation)
                .ToList();

            var loTrinhDisplay = loTrinhs.Select(lt => new
            {
                LoTrinhId = lt.LoTrinhId,
                Name = lt.TramDiNavigation.TenTram + " - " + lt.TramToiNavigation.TenTram
            }).ToList();

            ViewBag.LoTrinhId = new SelectList(loTrinhDisplay, "LoTrinhId", "Name", selectedLoTrinh);

            ViewBag.XeId = new SelectList(_context.Xe.ToList(), "XeId", "BienSoXe", selectedXe);
        }

    }
}
