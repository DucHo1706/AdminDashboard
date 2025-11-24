using AdminDashboard.Models;
using AdminDashboard.TransportDBContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace AdminDashboard.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class LoTrinhController : Controller
    {
        private readonly Db27524Context _context;

        public LoTrinhController(Db27524Context context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var loTrinhs = await _context.LoTrinh
                .Include(lt => lt.TramDiNavigation)
                .Include(lt => lt.TramToiNavigation)
                .ToListAsync();
            return View(loTrinhs);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loTrinh = await _context.LoTrinh
                .Include(lt => lt.TramDiNavigation)
                .Include(lt => lt.TramToiNavigation)
                .FirstOrDefaultAsync(m => m.LoTrinhId == id);
            if (loTrinh == null)
            {
                return NotFound();
            }

            return View(loTrinh);
        }

        public IActionResult Create()
        {
            ViewData["TramDi"] = new SelectList(_context.Tram, "IdTram", "TenTram");
            ViewData["TramToi"] = new SelectList(_context.Tram, "IdTram", "TenTram");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TramDi,TramToi,GiaVeCoDinh")] LoTrinh loTrinh)
        {
            ModelState.Remove("LoTrinhId");
            ModelState.Remove("TramDiNavigation");
            ModelState.Remove("TramToiNavigation");

            // KIỂM TRA GIÁ TRỊ - CHẶN HOÀN TOÀN SỐ ÂM VÀ < 5000
            if (!loTrinh.GiaVeCoDinh.HasValue)
            {
                ModelState.AddModelError("GiaVeCoDinh", "Giá vé là bắt buộc và phải lớn hơn hoặc bằng 5,000 VNĐ.");
                ViewData["TramDi"] = new SelectList(_context.Tram, "IdTram", "TenTram", loTrinh.TramDi);
                ViewData["TramToi"] = new SelectList(_context.Tram, "IdTram", "TenTram", loTrinh.TramToi);
                return View(loTrinh);
            }

            decimal originalValue = loTrinh.GiaVeCoDinh.Value;
            
            // CHẶN HOÀN TOÀN SỐ ÂM - KHÔNG CHO LƯU BẤT KỲ TRƯỜNG HỢP NÀO
            if (originalValue < 0)
            {
                ModelState.AddModelError("GiaVeCoDinh", "Giá vé không được là số âm! Vui lòng nhập số dương lớn hơn hoặc bằng 5,000 VNĐ.");
                ViewData["TramDi"] = new SelectList(_context.Tram, "IdTram", "TenTram", loTrinh.TramDi);
                ViewData["TramToi"] = new SelectList(_context.Tram, "IdTram", "TenTram", loTrinh.TramToi);
                return View(loTrinh);
            }
            
            // KIỂM TRA GIÁ TRỊ PHẢI >= 5000 - KHÔNG TỰ ĐỘNG SỬA, BẮT BUỘC NGƯỜI DÙNG NHẬP LẠI
            if (originalValue < 5000)
            {
                ModelState.AddModelError("GiaVeCoDinh", "Giá vé phải lớn hơn hoặc bằng 5,000 VNĐ. Vui lòng nhập lại!");
                ViewData["TramDi"] = new SelectList(_context.Tram, "IdTram", "TenTram", loTrinh.TramDi);
                ViewData["TramToi"] = new SelectList(_context.Tram, "IdTram", "TenTram", loTrinh.TramToi);
                return View(loTrinh);
            }

            // CHỈ CHO PHÉP LƯU NẾU GIÁ TRỊ >= 5000 VÀ KHÔNG PHẢI SỐ ÂM
            if (ModelState.IsValid)
            {
                if (loTrinh.TramDi == loTrinh.TramToi)
                {
                    ModelState.AddModelError("TramToi", "Trạm đến không được trùng với trạm đi.");
                    ViewData["TramDi"] = new SelectList(_context.Tram, "IdTram", "TenTram", loTrinh.TramDi);
                    ViewData["TramToi"] = new SelectList(_context.Tram, "IdTram", "TenTram", loTrinh.TramToi);
                    return View(loTrinh);
                }

                // KIỂM TRA LẠI LẦN CUỐI - KHÔNG CHO LƯU NẾU < 5000 HOẶC SỐ ÂM
                if (!loTrinh.GiaVeCoDinh.HasValue || loTrinh.GiaVeCoDinh.Value < 0 || loTrinh.GiaVeCoDinh.Value < 5000)
                {
                    ModelState.AddModelError("GiaVeCoDinh", "Giá vé phải lớn hơn hoặc bằng 5,000 VNĐ và không được là số âm. Vui lòng nhập lại!");
                    ViewData["TramDi"] = new SelectList(_context.Tram, "IdTram", "TenTram", loTrinh.TramDi);
                    ViewData["TramToi"] = new SelectList(_context.Tram, "IdTram", "TenTram", loTrinh.TramToi);
                    return View(loTrinh);
                }

                var newId = Guid.NewGuid().ToString();
                loTrinh.LoTrinhId = newId;

                _context.Add(loTrinh);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["TramDi"] = new SelectList(_context.Tram, "IdTram", "TenTram", loTrinh.TramDi);
            ViewData["TramToi"] = new SelectList(_context.Tram, "IdTram", "TenTram", loTrinh.TramToi);
            return View(loTrinh);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loTrinh = await _context.LoTrinh.FindAsync(id);
            if (loTrinh == null)
            {
                return NotFound();
            }

            ViewData["TramDi"] = new SelectList(_context.Tram, "IdTram", "TenTram", loTrinh.TramDi);
            ViewData["TramToi"] = new SelectList(_context.Tram, "IdTram", "TenTram", loTrinh.TramToi);
            return View(loTrinh);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("LoTrinhId,TramDi,TramToi,GiaVeCoDinh")] LoTrinh loTrinh)
        {
            if (id != loTrinh.LoTrinhId)
            {
                return NotFound();
            }

            ModelState.Remove("TramDiNavigation");
            ModelState.Remove("TramToiNavigation");

            // KIỂM TRA GIÁ TRỊ - CHẶN HOÀN TOÀN SỐ ÂM VÀ < 5000
            if (!loTrinh.GiaVeCoDinh.HasValue)
            {
                ModelState.AddModelError("GiaVeCoDinh", "Giá vé là bắt buộc và phải lớn hơn hoặc bằng 5,000 VNĐ.");
                ViewData["TramDi"] = new SelectList(_context.Tram, "IdTram", "TenTram", loTrinh.TramDi);
                ViewData["TramToi"] = new SelectList(_context.Tram, "IdTram", "TenTram", loTrinh.TramToi);
                return View(loTrinh);
            }

            decimal originalValue = loTrinh.GiaVeCoDinh.Value;
            
            // CHẶN HOÀN TOÀN SỐ ÂM - KHÔNG CHO LƯU BẤT KỲ TRƯỜNG HỢP NÀO
            if (originalValue < 0)
            {
                ModelState.AddModelError("GiaVeCoDinh", "Giá vé không được là số âm! Vui lòng nhập số dương lớn hơn hoặc bằng 5,000 VNĐ.");
                ViewData["TramDi"] = new SelectList(_context.Tram, "IdTram", "TenTram", loTrinh.TramDi);
                ViewData["TramToi"] = new SelectList(_context.Tram, "IdTram", "TenTram", loTrinh.TramToi);
                return View(loTrinh);
            }
            
            // KIỂM TRA GIÁ TRỊ PHẢI >= 5000 - KHÔNG TỰ ĐỘNG SỬA, BẮT BUỘC NGƯỜI DÙNG NHẬP LẠI
            if (originalValue < 5000)
            {
                ModelState.AddModelError("GiaVeCoDinh", "Giá vé phải lớn hơn hoặc bằng 5,000 VNĐ. Vui lòng nhập lại!");
                ViewData["TramDi"] = new SelectList(_context.Tram, "IdTram", "TenTram", loTrinh.TramDi);
                ViewData["TramToi"] = new SelectList(_context.Tram, "IdTram", "TenTram", loTrinh.TramToi);
                return View(loTrinh);
            }

            // CHỈ CHO PHÉP LƯU NẾU GIÁ TRỊ >= 5000 VÀ KHÔNG PHẢI SỐ ÂM
            if (ModelState.IsValid)
            {
                if (loTrinh.TramDi == loTrinh.TramToi)
                {
                    ModelState.AddModelError("TramToi", "Trạm đến không được trùng với trạm đi.");
                    ViewData["TramDi"] = new SelectList(_context.Tram, "IdTram", "TenTram", loTrinh.TramDi);
                    ViewData["TramToi"] = new SelectList(_context.Tram, "IdTram", "TenTram", loTrinh.TramToi);
                    return View(loTrinh);
                }

                // KIỂM TRA LẠI LẦN CUỐI - KHÔNG CHO LƯU NẾU < 5000 HOẶC SỐ ÂM
                if (!loTrinh.GiaVeCoDinh.HasValue || loTrinh.GiaVeCoDinh.Value < 0 || loTrinh.GiaVeCoDinh.Value < 5000)
                {
                    ModelState.AddModelError("GiaVeCoDinh", "Giá vé phải lớn hơn hoặc bằng 5,000 VNĐ và không được là số âm. Vui lòng nhập lại!");
                    ViewData["TramDi"] = new SelectList(_context.Tram, "IdTram", "TenTram", loTrinh.TramDi);
                    ViewData["TramToi"] = new SelectList(_context.Tram, "IdTram", "TenTram", loTrinh.TramToi);
                    return View(loTrinh);
                }

                try
                {
                    _context.Update(loTrinh);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LoTrinhExists(loTrinh.LoTrinhId))
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

            ViewData["TramDi"] = new SelectList(_context.Tram, "IdTram", "TenTram", loTrinh.TramDi);
            ViewData["TramToi"] = new SelectList(_context.Tram, "IdTram", "TenTram", loTrinh.TramToi);
            return View(loTrinh);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loTrinh = await _context.LoTrinh
                .Include(lt => lt.TramDiNavigation)
                .Include(lt => lt.TramToiNavigation)
                .FirstOrDefaultAsync(m => m.LoTrinhId == id);
            if (loTrinh == null)
            {
                return NotFound();
            }

            return View(loTrinh);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var loTrinh = await _context.LoTrinh.FindAsync(id);
            if (loTrinh == null)
            {
                return NotFound();
            }
            _context.LoTrinh.Remove(loTrinh);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LoTrinhExists(string id)
        {
            return _context.LoTrinh.Any(e => e.LoTrinhId == id);
        }
    }
}