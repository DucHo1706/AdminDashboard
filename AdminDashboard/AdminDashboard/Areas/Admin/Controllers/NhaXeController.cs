using AdminDashboard.Areas.Admin.Models;
using AdminDashboard.Models;
using AdminDashboard.TransportDBContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminDashboard.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class NhaXeController : Controller
    {
        private readonly Db27524Context _context;

        public NhaXeController(Db27524Context context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var listNhaXe = await _context.NhaXe.ToListAsync();
            return View(listNhaXe);
        }
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var nhaXe = await _context.NhaXe.FirstOrDefaultAsync(m => m.NhaXeId == id);
            if (nhaXe == null) return NotFound();

            return View(nhaXe);
        }
        public IActionResult Create()
        {
            return View();
        }

        // POST: Xử lý dữ liệu tạo mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateNhaXeViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _context.NguoiDung.AnyAsync(u => u.Email == model.EmailChuXe))
                {
                    ModelState.AddModelError("EmailChuXe", "Email này đã có người dùng sử dụng.");
                    return View(model);
                }

                var nhaXe = new AdminDashboard.Models.NhaXe
                {
                    NhaXeId = Guid.NewGuid().ToString("N"), 
                    TenNhaXe = model.TenNhaXe,
                    SoDienThoai = model.SoDienThoaiNhaXe,
                    DiaChi = model.DiaChi,
                    TrangThai = 1 
                };

                _context.NhaXe.Add(nhaXe);
                await _context.SaveChangesAsync();

                var chuXe = new NguoiDung
                {
                    UserId = Guid.NewGuid().ToString(),
                    HoTen = model.HoTenChuXe,
                    Email = model.EmailChuXe,
                    MatKhau = model.MatKhauMacDinh, 
                    TrangThai = TrangThaiNguoiDung.HoatDong,
                    NhaXeId = nhaXe.NhaXeId 
                };
                _context.NguoiDung.Add(chuXe);

                var roleChuXe = await _context.VaiTro.FirstOrDefaultAsync(r => r.TenVaiTro == "ChuNhaXe");
                if (roleChuXe != null)
                {
                    _context.UserRole.Add(new UserRole
                    {
                        UserId = chuXe.UserId,
                        RoleId = roleChuXe.RoleId
                    });
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var nhaXe = await _context.NhaXe.FindAsync(id);
            if (nhaXe == null) return NotFound();

            return View(nhaXe);
        }

        // POST: Xử lý lưu thay đổi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, AdminDashboard.Models.NhaXe nhaXeInput)
        {
            if (id != nhaXeInput.NhaXeId)
            {
                return NotFound();
            }
            var nhaXeGoc = await _context.NhaXe.FindAsync(id);

            if (nhaXeGoc == null)
            {
                return NotFound();
            }
            nhaXeGoc.TenNhaXe = nhaXeInput.TenNhaXe;
            nhaXeGoc.SoDienThoai = nhaXeInput.SoDienThoai;
            nhaXeGoc.DiaChi = nhaXeInput.DiaChi;
            nhaXeGoc.TrangThai = nhaXeInput.TrangThai;
            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NhaXeExists(nhaXeInput.NhaXeId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi lưu: " + ex.Message);
            }
            return View(nhaXeInput);
        }
        // GET: Xác nhận xóa
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();

            var nhaXe = await _context.NhaXe.FirstOrDefaultAsync(m => m.NhaXeId == id);
            if (nhaXe == null) return NotFound();

            return View(nhaXe);
        }

        // POST: Thực hiện xóa
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var nhaXe = await _context.NhaXe.FindAsync(id);
            if (nhaXe != null)
            {
                nhaXe.TrangThai = 0; 
                _context.Update(nhaXe);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        private bool NhaXeExists(string id)
        {
            return (_context.NhaXe?.Any(e => e.NhaXeId == id)).GetValueOrDefault();
        }
    }
}