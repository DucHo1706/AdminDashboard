using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdminDashboard.Models;
using AdminDashboard.TransportDBContext;
using Microsoft.AspNetCore.Authorization;

namespace AdminDashboard.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = "Admin")]
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
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "ID trạm không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var tram = await _context.Tram.FindAsync(id);
                if (tram == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy trạm cần xóa.";
                    return RedirectToAction(nameof(Index));
                }

                // Kiểm tra xem có lộ trình nào đang sử dụng trạm này không
                var coLoTrinhDi = await _context.LoTrinh.AnyAsync(lt => lt.TramDi == id);
                var coLoTrinhDen = await _context.LoTrinh.AnyAsync(lt => lt.TramToi == id);
                
                if (coLoTrinhDi || coLoTrinhDen)
                {
                    TempData["ErrorMessage"] = "Không thể xóa trạm này vì có lộ trình đang sử dụng.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Tram.Remove(tram);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Đã xóa trạm thành công!";
            }
            catch (DbUpdateException dbEx)
            {
                string errorMessage = "Không thể xóa trạm này vì có dữ liệu liên quan.";
                if (dbEx.InnerException != null)
                {
                    errorMessage += " Chi tiết: " + dbEx.InnerException.Message;
                }
                TempData["ErrorMessage"] = errorMessage;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}