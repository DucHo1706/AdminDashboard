using AdminDashboard.Models;
using AdminDashboard.TransportDBContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AdminDashboard.Areas.NhaXe.Controllers
{
    [Area("NhaXe")]
    // [Authorize(Roles = "ChuNhaXe")] // Bật lại sau khi test xong
    public class TaiXeController : Controller
    {
        private readonly Db27524Context _context;

        public TaiXeController(Db27524Context context)
        {
            _context = context;
        }

        // Helper: Lấy ID nhà xe từ Cookie
        private string GetCurrentNhaXeId()
        {
            return User.FindFirst("NhaXeId")?.Value;
        }

        // 1. Danh sách Tài xế (Chỉ hiện nhân viên của mình)
        public async Task<IActionResult> Index()
        {
            var nhaXeId = GetCurrentNhaXeId();
            if (string.IsNullOrEmpty(nhaXeId)) return RedirectToAction("Login", "Auth", new { area = "" });

            // Lấy User có Role='TaiXe' VÀ thuộc NhaXeId này
            var listTaiXe = await _context.NguoiDung
                .Where(u => u.NhaXeId == nhaXeId)
                .Where(u => u.UserRoles.Any(ur => ur.VaiTro.TenVaiTro == "TaiXe"))
                .ToListAsync();

            return View(listTaiXe);
        }

        // 2. Tạo Tài xế Mới (GET)
        public IActionResult Create()
        {
            return View();
        }

        // 3. Xử lý Tạo Tài xế (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NguoiDung model)
        {
            var nhaXeId = GetCurrentNhaXeId();
            if (string.IsNullOrEmpty(nhaXeId)) return RedirectToAction("Login", "Auth", new { area = "" });

            // Kiểm tra trùng Email
            if (await _context.NguoiDung.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                return View(model);
            }

            // Tạo User mới
            var taiXe = new NguoiDung
            {
                UserId = Guid.NewGuid().ToString(),
                HoTen = model.HoTen,
                Email = model.Email,
                SoDienThoai = model.SoDienThoai,
                MatKhau = model.MatKhau, // Lưu ý: Nên mã hóa mật khẩu
                TrangThai = TrangThaiNguoiDung.HoatDong, // 1: Hoạt động
                NgaySinh = model.NgaySinh,

                // QUAN TRỌNG: Gán vào nhà xe này
                NhaXeId = nhaXeId
            };

            _context.NguoiDung.Add(taiXe);

            // Gán Role "TaiXe"
            var roleTaiXe = await _context.VaiTro.FirstOrDefaultAsync(x => x.TenVaiTro == "TaiXe");
            if (roleTaiXe != null)
            {
                _context.UserRole.Add(new UserRole
                {
                    UserId = taiXe.UserId,
                    RoleId = roleTaiXe.RoleId
                });
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Thêm tài xế thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}