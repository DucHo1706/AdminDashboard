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
            try
            {
                var dsXe = _context.Xe
                    .Include(x => x.LoaiXe)
                    .Include(x => x.DanhSachGhe) // Include danh sách ghế
                    .AsNoTracking(); // Thêm AsNoTracking để tăng performance

                return View(await dsXe.ToListAsync());
            }
            catch (Exception ex)
            {
                // Log lỗi và hiển thị thông báo thân thiện
                Console.WriteLine($"Lỗi khi tải danh sách xe: {ex.Message}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách xe.";
                return View(new List<Xe>());
            }
        }

        // GET: Xe/Create
        public IActionResult Create()
        {
            ViewData["LoaiXeId"] = new SelectList(_context.LoaiXe, "LoaiXeId", "TenLoaiXe");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BienSoXe,LoaiXeId,SoLuongGhe")] Xe xe)
        {
            ModelState.Remove("XeId");
            ModelState.Remove("DanhSachGhe");
            ModelState.Remove("LoaiXe");

            if (ModelState.IsValid)
            {
                // Tạo ID trước khi vào transaction
                xe.XeId = Guid.NewGuid().ToString("N").Substring(0, 8);

                // Lấy execution strategy
                var strategy = _context.Database.CreateExecutionStrategy();

                try
                {
                    // Sử dụng execution strategy để thực thi transaction
                    await strategy.ExecuteAsync(async () =>
                    {
                        using var transaction = await _context.Database.BeginTransactionAsync();
                        try
                        {
                            // Thêm xe vào database
                            _context.Add(xe);
                            await _context.SaveChangesAsync();

                            // Tạo danh sách ghế
                            await TaoDanhSachGhe(xe.XeId, xe.SoLuongGhe);

                            await transaction.CommitAsync();
                            TempData["SuccessMessage"] = "Thêm xe và tạo ghế thành công!";
                        }
                        catch (Exception)
                        {
                            await transaction.RollbackAsync();
                            throw;
                        }
                    });

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi lưu dữ liệu: " + ex.Message);
                    Console.WriteLine($"Chi tiết lỗi: {ex.InnerException?.Message}");
                }
            }

            ViewData["LoaiXeId"] = new SelectList(_context.LoaiXe, "LoaiXeId", "TenLoaiXe", xe.LoaiXeId);
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
                .Include(x => x.DanhSachGhe) // Load danh sách ghế
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
                    TempData["SuccessMessage"] = "Cập nhật xe thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!XeExists(xe.XeId))
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
                .Include(x => x.LoaiXe)
                .Include(x => x.DanhSachGhe)
                .FirstOrDefaultAsync(m => m.XeId == id);

            if (xe == null)
            {
                return NotFound();
            }

            return View(xe);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            try
            {
                await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        var xe = await _context.Xe
                            .Include(x => x.DanhSachGhe)
                            .FirstOrDefaultAsync(x => x.XeId == id);

                        if (xe != null)
                        {
                            // Xóa tất cả ghế trước
                            if (xe.DanhSachGhe != null && xe.DanhSachGhe.Any())
                            {
                                _context.Ghe.RemoveRange(xe.DanhSachGhe);
                            }

                            // Sau đó xóa xe
                            _context.Xe.Remove(xe);
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        TempData["SuccessMessage"] = "Xóa xe và các ghế liên quan thành công!";
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xóa: " + ex.Message;
                return RedirectToAction(nameof(Delete), new { id });
            }

            return RedirectToAction(nameof(Index));
        }

        private bool XeExists(string id)
        {
            return _context.Xe.Any(e => e.XeId == id);
        }

        private async Task<bool> TaoDanhSachGhe(string xeId, int soLuongGhe)
        {
            try
            {
                var danhSachGhe = new List<Ghe>();

                for (int i = 1; i <= soLuongGhe; i++)
                {
                    var ghe = new Ghe
                    {
                        GheID = Guid.NewGuid().ToString("N").Substring(0, 10),
                        XeId = xeId,
                        SoGhe = i.ToString("D2"),
                        TrangThai = "Trống"
                    };
                    danhSachGhe.Add(ghe);
                }

                await _context.Ghe.AddRangeAsync(danhSachGhe);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log lỗi chi tiết
                Console.WriteLine($"Lỗi khi tạo ghế: {ex.Message}");
                return false;
            }
        }

        // Action để xem danh sách ghế của xe
        public async Task<IActionResult> DanhSachGhe(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var danhSachGhe = await _context.Ghe
                .Where(g => g.XeId == id)
                .OrderBy(g => g.SoGhe)
                .ToListAsync();

            ViewBag.XeId = id;
            var xe = await _context.Xe.FindAsync(id);
            ViewBag.BienSoXe = xe?.BienSoXe;

            return View(danhSachGhe);
        }
    }
}