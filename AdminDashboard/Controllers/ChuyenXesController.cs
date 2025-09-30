using System.Linq;
using System.Threading.Tasks;
using AdminDashboard.Models;
using AdminDashboard.TransportDBContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AdminDashboard.Controllers
{
    public class ChuyenXesController : Controller
    {
        private readonly Db27524Context _context; // Thay 'ApplicationDbContext' bằng tên DbContext của bạn

        public ChuyenXesController(Db27524Context context)
        {
            _context = context;
        }

        // GET: ChuyenXes
        // Hành động hiển thị danh sách tất cả chuyến xe
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách chuyến xe, bao gồm cả thông tin Lộ Trình và Xe liên quan
            var chuyenXes = _context.ChuyenXe
                                    .Include(c => c.LoTrinh)
                                    .Include(c => c.Xe);
            return View(await chuyenXes.ToListAsync());
        }

        // GET: ChuyenXes/Details/5
        // Hành động xem chi tiết một chuyến xe
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chuyenXe = await _context.ChuyenXe
                .Include(c => c.LoTrinh)
                .Include(c => c.Xe)
                .FirstOrDefaultAsync(m => m.ChuyenId == id);

            if (chuyenXe == null)
            {
                return NotFound();
            }

            return View(chuyenXe);
        }

        // GET: ChuyenXes/Create
        // Hành động hiển thị form để tạo mới chuyến xe
        public IActionResult Create()
        {
            // Gửi danh sách Lộ trình và Xe sang View để người dùng chọn
            ViewData["LoTrinhId"] = new SelectList(_context.LoTrinh, "LoTrinhId", "TenLoTrinh"); // Giả sử model LoTrinh có thuộc tính 'TenLoTrinh'
            ViewData["XeId"] = new SelectList(_context.Xe, "XeId", "BienSo"); // Giả sử model Xe có thuộc tính 'BienSo'
            return View();
        }

        // POST: ChuyenXes/Create
        // Hành động xử lý dữ liệu từ form tạo mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ChuyenId,LoTrinhId,XeId,NgayDi,GioDi,GioDenDuKien,TrangThai")] ChuyenXe chuyenXe)
        {
            if (ModelState.IsValid)
            {
                _context.Add(chuyenXe);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // Nếu model không hợp lệ, trả lại form với dữ liệu đã nhập và danh sách dropdown
            ViewData["LoTrinhId"] = new SelectList(_context.LoTrinh, "LoTrinhId", "TenLoTrinh", chuyenXe.LoTrinhId);
            ViewData["XeId"] = new SelectList(_context.Xe, "XeId", "BienSo", chuyenXe.XeId);
            return View(chuyenXe);
        }

        // GET: ChuyenXes/Edit/5
        // Hành động hiển thị form để chỉnh sửa chuyến xe
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chuyenXe = await _context.ChuyenXe.FindAsync(id);
            if (chuyenXe == null)
            {
                return NotFound();
            }
            ViewData["LoTrinhId"] = new SelectList(_context.LoTrinh, "LoTrinhId", "TenLoTrinh", chuyenXe.LoTrinhId);
            ViewData["XeId"] = new SelectList(_context.Xe, "XeId", "BienSo", chuyenXe.XeId);
            return View(chuyenXe);
        }

        // POST: ChuyenXes/Edit/5
        // Hành động xử lý dữ liệu từ form chỉnh sửa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("ChuyenId,LoTrinhId,XeId,NgayDi,GioDi,GioDenDuKien,TrangThai")] ChuyenXe chuyenXe)
        {
            if (id != chuyenXe.ChuyenId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chuyenXe);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChuyenXeExists(chuyenXe.ChuyenId))
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
            ViewData["LoTrinhId"] = new SelectList(_context.LoTrinh, "LoTrinhId", "TenLoTrinh", chuyenXe.LoTrinhId);
            ViewData["XeId"] = new SelectList(_context.Xe, "XeId", "BienSo", chuyenXe.XeId);
            return View(chuyenXe);
        }

        // GET: ChuyenXes/Delete/5
        // Hành động hiển thị trang xác nhận xóa
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chuyenXe = await _context.ChuyenXe
                .Include(c => c.LoTrinh)
                .Include(c => c.Xe)
                .FirstOrDefaultAsync(m => m.ChuyenId == id);
            if (chuyenXe == null)
            {
                return NotFound();
            }

            return View(chuyenXe);
        }

        // POST: ChuyenXes/Delete/5
        // Hành động xác nhận và thực hiện xóa
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var chuyenXe = await _context.ChuyenXe.FindAsync(id);
            _context.ChuyenXe.Remove(chuyenXe);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Phương thức private để kiểm tra sự tồn tại của chuyến xe
        private bool ChuyenXeExists(string id)
        {
            return _context.ChuyenXe.Any(e => e.ChuyenId == id);
        }
    }
}