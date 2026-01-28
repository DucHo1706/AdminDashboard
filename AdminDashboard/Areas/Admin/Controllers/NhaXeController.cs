using AdminDashboard.Areas.Admin.Models;
using AdminDashboard.Models;
using AdminDashboard.TransportDBContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminDashboard.Areas.Admin.Controllers
{
    [Area("Admin")]
    // [Authorize(Roles = "Admin")] // Bật cái này sau khi làm xong đăng nhập
    public class NhaXeController : Controller
    {
        private readonly Db27524Context _context;

        public NhaXeController(Db27524Context context)
        {
            _context = context;
        }

        // 1. Danh sách nhà xe
        public async Task<IActionResult> Index()
        {
            var listNhaXe = await _context.NhaXe.ToListAsync();
            return View(listNhaXe);
        }

        // 2. Form tạo mới (GET)
        public IActionResult Create()
        {
            return View();
        }

        // 3. Xử lý tạo mới (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateNhaXeViewModel model)
        {
            if (ModelState.IsValid)
            {
                // A. Kiểm tra trùng Email
                if (await _context.NguoiDung.AnyAsync(u => u.Email == model.EmailChuXe))
                {
                    ModelState.AddModelError("EmailChuXe", "Email này đã có người dùng.");
                    return View(model);
                }

                // B. Tạo Nhà Xe
                var nhaXe = new AdminDashboard.Models.NhaXe
                {
                    NhaXeId = Guid.NewGuid().ToString("N"),
                    TenNhaXe = model.TenNhaXe,
                    SoDienThoai = model.SoDienThoaiNhaXe,
                    DiaChi = model.DiaChi,
                    TrangThai = 1 // 1: Hoạt động luôn (vì do Admin tạo mà)
                }; 

                _context.NhaXe.Add(nhaXe);
                await _context.SaveChangesAsync(); // Lưu để có NhaXeId

                // C. Tạo Tài khoản Chủ xe
                var chuXe = new NguoiDung
                {
                    UserId = Guid.NewGuid().ToString(), // ID User luôn là Guid
                    HoTen = model.HoTenChuXe,
                    Email = model.EmailChuXe,
                    MatKhau = model.MatKhauMacDinh, // Nhớ mã hóa nếu cần
                    TrangThai = TrangThaiNguoiDung.HoatDong,
                    NhaXeId = nhaXe.NhaXeId // <--- LIÊN KẾT QUAN TRỌNG NHẤT
                };
                _context.NguoiDung.Add(chuXe);

                // D. Gán quyền "ChuNhaXe"
                // Tìm ID của Role "ChuNhaXe" trong DB
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
    }
}