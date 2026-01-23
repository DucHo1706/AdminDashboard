using AdminDashboard.Models;
using AdminDashboard.TransportDBContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AdminDashboard.Areas.NhaXe.Controllers
{
    [Area("NhaXe")]
    // [Authorize(Roles = "ChuNhaXe")
    public class XeController : Controller
    {
        private readonly Db27524Context _context;

        public XeController(Db27524Context context)
        {
            _context = context;
        }

        // Helper lấy NhaXeId hiện tại
        private string GetCurrentNhaXeId()
        {
            return User.FindFirst("NhaXeId")?.Value;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var nhaXeId = GetCurrentNhaXeId();
                if (string.IsNullOrEmpty(nhaXeId))
                {
                    // Nếu chưa đăng nhập hoặc không phải nhà xe, đá về login
                    return RedirectToAction("Login", "Auth", new { area = "" });
                }

                var dsXe = _context.Xe
                    .Include(x => x.LoaiXe)
                    .Include(x => x.DanhSachGhe)
                    .Where(x => x.NhaXeId == nhaXeId) // <--- QUAN TRỌNG: Chỉ hiện xe của mình
                    .AsNoTracking();

                return View(await dsXe.ToListAsync());
            }
            catch (Exception ex)
            {
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
            // 1. LẤY ID NHÀ XE TỪ COOKIE
            var currentNhaXeId = GetCurrentNhaXeId();
            if (string.IsNullOrEmpty(currentNhaXeId))
            {
                ModelState.AddModelError("", "Không tìm thấy thông tin Nhà xe. Vui lòng đăng nhập lại.");
                ViewData["LoaiXeId"] = new SelectList(_context.LoaiXe, "LoaiXeId", "TenLoaiXe", xe.LoaiXeId);
                return View(xe);
            }

            // 2. GÁN ID VÀO XE
            xe.NhaXeId = currentNhaXeId;

            // 3. BỎ QUA VALIDATION CHO CÁC TRƯỜNG NÀY
            ModelState.Remove("XeId");
            ModelState.Remove("DanhSachGhe");
            ModelState.Remove("LoaiXe");
            ModelState.Remove("NhaXe");   // <--- Thêm cái này
            ModelState.Remove("NhaXeId"); // <--- Thêm cái này

            if (ModelState.IsValid)
            {
                // Tạo ID
                xe.XeId = Guid.NewGuid().ToString("N").Substring(0, 8);

                var strategy = _context.Database.CreateExecutionStrategy();

                try
                {
                    await strategy.ExecuteAsync(async () =>
                    {
                        using var transaction = await _context.Database.BeginTransactionAsync();
                        try
                        {
                            _context.Add(xe);
                            await _context.SaveChangesAsync();

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
                }
            }

            ViewData["LoaiXeId"] = new SelectList(_context.LoaiXe, "LoaiXeId", "TenLoaiXe", xe.LoaiXeId);
            return View(xe);
        }

        // ... CÁC HÀM KHÁC (Details, Edit, Delete...) GIỮ NGUYÊN ...
        // ... CHỈ CẦN THÊM CHECK QUYỀN SỞ HỮU NẾU CẦN THIẾT ...

        // Ví dụ sửa hàm Edit để check quyền
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var nhaXeId = GetCurrentNhaXeId();
            var xe = await _context.Xe.FindAsync(id);

            // Check: Xe có tồn tại và CÓ PHẢI CỦA MÌNH KHÔNG
            if (xe == null || xe.NhaXeId != nhaXeId)
            {
                return NotFound();
            }

            ViewData["LoaiXeId"] = new SelectList(_context.LoaiXe, "LoaiXeId", "TenLoaiXe", xe.LoaiXeId);
            return View(xe);
        }

        // ... Các hàm DeleteConfirmed, TaoDanhSachGhe giữ nguyên như code cũ của bạn ...
        // Chỉ lưu ý bổ sung logic check `if (xe.NhaXeId != nhaXeId)` ở các hàm thao tác ID cụ thể.

        // GET: Xe/Details/
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var nhaXeId = GetCurrentNhaXeId();

            var xe = await _context.Xe
                .Include(x => x.LoaiXe)
                .Include(x => x.DanhSachGhe)
                .FirstOrDefaultAsync(m => m.XeId == id);

            if (xe == null || xe.NhaXeId != nhaXeId) return NotFound();

            return View(xe);
        }

        // GET: Xe/Delete/
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();
            var nhaXeId = GetCurrentNhaXeId();

            var xe = await _context.Xe
                .Include(x => x.LoaiXe)
                .Include(x => x.DanhSachGhe)
                .FirstOrDefaultAsync(m => m.XeId == id);

            if (xe == null || xe.NhaXeId != nhaXeId) return NotFound();

            return View(xe);
        }

        // POST: Xe/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("XeId,BienSoXe,LoaiXeId")] Xe xe)
        {
            if (id != xe.XeId) return NotFound();

            // Lấy lại NhaXeId gán vào, vì form edit có thể không gửi NhaXeId lên
            var nhaXeId = GetCurrentNhaXeId();

            // Query lại xe cũ để chắc chắn nó là của mình
            var oldXe = await _context.Xe.AsNoTracking().FirstOrDefaultAsync(x => x.XeId == id);
            if (oldXe == null || oldXe.NhaXeId != nhaXeId) return NotFound();

            // Gán lại NhaXeId cho object xe đang edit
            xe.NhaXeId = nhaXeId;

            // Xóa validate NhaXe
            ModelState.Remove("NhaXe");
            ModelState.Remove("NhaXeId");
            ModelState.Remove("LoaiXe");
            ModelState.Remove("DanhSachGhe");

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
                    if (!XeExists(xe.XeId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["LoaiXeId"] = new SelectList(_context.LoaiXe, "LoaiXeId", "TenLoaiXe", xe.LoaiXeId);
            return View(xe);
        }

        // ... Các phần DeleteConfirmed, TaoDanhSachGhe, DanhSachGhe, XeExists giữ nguyên ...
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "ID xe không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            var nhaXeId = GetCurrentNhaXeId();
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

                        // THÊM CHECK QUYỀN
                        if (xe == null || xe.NhaXeId != nhaXeId)
                        {
                            TempData["ErrorMessage"] = "Không tìm thấy xe hoặc bạn không có quyền xóa.";
                            await transaction.RollbackAsync();
                            return;
                        }

                        // ... (Giữ nguyên logic kiểm tra chuyến xe, vé, xóa ghế của bạn) ...
                        var coChuyenXe = await _context.ChuyenXe.AnyAsync(cx => cx.XeId == id);
                        if (coChuyenXe)
                        {
                            TempData["ErrorMessage"] = "Không thể xóa xe này vì có chuyến xe đang sử dụng.";
                            await transaction.RollbackAsync();
                            return;
                        }

                        var danhSachGheId = xe.DanhSachGhe?.Select(g => g.GheID).ToList() ?? new List<string>();
                        if (danhSachGheId.Any())
                        {
                            var coVe = await _context.Ve
                                .Include(v => v.DonHang)
                                .AnyAsync(v => danhSachGheId.Contains(v.GheID) &&
                                             (v.DonHang.TrangThaiThanhToan == "Đã thanh toán" ||
                                              v.DonHang.TrangThaiThanhToan == "Da thanh toan"));
                            if (coVe)
                            {
                                TempData["ErrorMessage"] = "Không thể xóa xe này vì có vé đã được bán.";
                                await transaction.RollbackAsync();
                                return;
                            }
                        }

                        if (xe.DanhSachGhe != null && xe.DanhSachGhe.Any())
                        {
                            _context.Ghe.RemoveRange(xe.DanhSachGhe);
                        }

                        _context.Xe.Remove(xe);

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        TempData["SuccessMessage"] = "Xóa xe thành công!";
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
            }

            return RedirectToAction(nameof(Index));
        }

        private bool XeExists(string id)
        {
            return _context.Xe.Any(e => e.XeId == id);
        }

        private async Task<bool> TaoDanhSachGhe(string xeId, int soLuongGhe)
        {
            // ... Giữ nguyên code cũ của bạn ...
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
                Console.WriteLine($"Lỗi khi tạo ghế: {ex.Message}");
                return false;
            }
        }

        public async Task<IActionResult> DanhSachGhe(string id)
        {
            if (id == null) return NotFound();
            var nhaXeId = GetCurrentNhaXeId();

            var xe = await _context.Xe.FindAsync(id);
            if (xe == null || xe.NhaXeId != nhaXeId) return NotFound();

            var danhSachGhe = await _context.Ghe
                .Where(g => g.XeId == id)
                .OrderBy(g => g.SoGhe)
                .ToListAsync();

            ViewBag.XeId = id;
            ViewBag.BienSoXe = xe?.BienSoXe;

            return View(danhSachGhe);
        }
    }
}