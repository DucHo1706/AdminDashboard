﻿using AdminDashboard.Models;
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

            if (ModelState.IsValid)
            {
                if (loTrinh.TramDi == loTrinh.TramToi)
                {
                    ModelState.AddModelError("TramToi", "Trạm đến không được trùng với trạm đi.");
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

            if (ModelState.IsValid)
            {
                if (loTrinh.TramDi == loTrinh.TramToi)
                {
                    ModelState.AddModelError("TramToi", "Trạm đến không được trùng với trạm đi.");
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