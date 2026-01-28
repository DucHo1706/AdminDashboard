using AdminDashboard.Models;
using AdminDashboard.TransportDBContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq; // Cần thêm để sử dụng Join
using System.Threading.Tasks;

namespace AdminDashboard.Controllers
{
    public class KhachHangController : Controller
    {
        private readonly Db27524Context _context;
        private const string KHACH_HANG_ROLE = "KhachHang"; 

        public KhachHangController(Db27524Context context)
        {
            _context = context;
        }

        // GET: KhachHang
        public async Task<IActionResult> Index()
        {
            // Lấy RoleId của "KhachHang"
            var khachHangRoleId = await _context.VaiTro
                .Where(r => r.TenVaiTro == KHACH_HANG_ROLE)
                .Select(r => r.RoleId)
                .FirstOrDefaultAsync();

            if (khachHangRoleId == null)
            {             
                ViewBag.ErrorMessage = "Không tìm thấy vai trò Khách Hàng trong hệ thống.";
                return View(new List<NguoiDung>()); 
            }

            // Lấy danh sách người dùng có vai trò là "KhachHang"
            var khachHangs = await _context.UserRole
                .Where(ur => ur.RoleId == khachHangRoleId)
                .Join(_context.NguoiDung, 
                      userRole => userRole.UserId,
                      nguoiDung => nguoiDung.UserId,
                      (userRole, nguoiDung) => nguoiDung) 
                .ToListAsync();

            return View(khachHangs);
        }

        // GET: KhachHang/Details/U12345
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            // Tìm NguoiDung theo UserId
            var nguoiDung = await _context.NguoiDung.FirstOrDefaultAsync(m => m.UserId == id);

            if (nguoiDung == null) return NotFound();

            return View(nguoiDung);
        }

        // Chức năng TẠO MỚI đã được loại bỏ
        // Lý do: Việc tạo người dùng mới cần có mật khẩu và các thông tin phức tạp khác,
        // nên thực hiện qua trang Đăng Ký (Register) trong AuthController để đảm bảo tính toàn vẹn dữ liệu.

        // GET: KhachHang/Edit/U12345
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var nguoiDung = await _context.NguoiDung.FindAsync(id);

            if (nguoiDung == null) return NotFound();

            return View(nguoiDung);
        }

        // POST: KhachHang/Edit/U12345
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("UserId,HoTen,Email,SoDienThoai,NgaySinh,TrangThai")] NguoiDung nguoiDung)
        {
            if (id != nguoiDung.UserId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy lại mật khẩu cũ vì nó không được gửi từ form
                    var userInDb = await _context.NguoiDung.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == id);
                    if (userInDb != null)
                    {
                        nguoiDung.MatKhau = userInDb.MatKhau;
                    }

                    _context.Update(nguoiDung);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.NguoiDung.Any(e => e.UserId == nguoiDung.UserId))
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
            return View(nguoiDung);
        }

        // GET: KhachHang/Delete/U12345
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();

            var nguoiDung = await _context.NguoiDung.FirstOrDefaultAsync(m => m.UserId == id);

            if (nguoiDung == null) return NotFound();

            return View(nguoiDung);
        }

        // POST: KhachHang/Delete/U12345
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var nguoiDung = await _context.NguoiDung.FindAsync(id);
            if (nguoiDung != null)
            {
                // Xóa cả các bản ghi liên quan trong UserRole để tránh lỗi khóa ngoại
                var userRoles = _context.UserRole.Where(ur => ur.UserId == id);
                _context.UserRole.RemoveRange(userRoles);

                _context.NguoiDung.Remove(nguoiDung);

                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}