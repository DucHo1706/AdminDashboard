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
			// Chuẩn bị dữ liệu cho các dropdown list
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
				// Logic tự động sinh Id mới
				var lastItem = await _context.ChuyenXe.OrderByDescending(c => c.ChuyenId).FirstOrDefaultAsync();
				string newId = "C000000001";
				if (lastItem != null)
				{
					int lastNumber = int.Parse(lastItem.ChuyenId.Substring(1));
					newId = "C" + (lastNumber + 1).ToString("D9"); // D9 để đảm bảo có 9 chữ số
				}
				chuyenXe.ChuyenId = newId;

				_context.Add(chuyenXe);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}

			// Nếu model không hợp lệ, tải lại dropdown và hiển thị lại form
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
			// Giả định Model LoTrinh có thuộc tính TenLoTrinh và Xe có thuộc tính BienSoXe để hiển thị
			ViewData["LoTrinhId"] = new SelectList(_context.Set<LoTrinh>(), "LoTrinhId", "TenLoTrinh", selectedLoTrinh);
			ViewData["XeId"] = new SelectList(_context.Set<Xe>(), "XeId", "BienSoXe", selectedXe);
		}
	}
}
