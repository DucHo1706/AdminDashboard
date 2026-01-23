using AdminDashboard.Models;
using AdminDashboard.Models.ViewModels;
using AdminDashboard.Services;
using AdminDashboard.TransportDBContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminDashboard.Areas.NhaXe.Controllers
{
    [Area("NhaXe")]
    // [Authorize(Roles = "ChuNhaXe")] // Bạn bỏ comment dòng này sau khi đã chạy code tạo Role
    public class NhanVienController : Controller
    {
        private readonly Db27524Context _context;
        private readonly INhanVienService _nhanVienService;

        public NhanVienController(Db27524Context context, INhanVienService nhanVienService)
        {
            _context = context;
            _nhanVienService = nhanVienService;
        }

        // Helper: Lấy ID Nhà xe từ User đang đăng nhập
        private string NhaXeId => User.FindFirst("NhaXeId")?.Value;

        // 1. DANH SÁCH NHÂN VIÊN
        public async Task<IActionResult> Index()
        {
            if (string.IsNullOrEmpty(NhaXeId))
            {
                // Nếu chưa đăng nhập hoặc không phải nhà xe -> Đá về trang Login
                return RedirectToAction("Login", "Auth", new { area = "" });
            }

            // Lấy danh sách nhân viên thuộc nhà xe này
            var list = await _context.NhanVien
                .Where(nv => nv.NhaXeId == NhaXeId)
                .OrderByDescending(nv => nv.NgayVaoLam)
                .ToListAsync();

            return View(list);
        }

        // 2. TẠO MỚI (Giao diện)
        public IActionResult Create()
        {
            return View();
        }

        // 3. TẠO MỚI (Xử lý logic)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaoNhanVienRequest request)
        {
            if (ModelState.IsValid)
            {
                // Gọi Service để xử lý tất cả (Tạo User, Upload ảnh, Lưu DB...)
                string result = await _nhanVienService.TaoNhanVienAsync(request, NhaXeId);

                if (result == "Success")
                {
                    TempData["SuccessMessage"] = "Thêm nhân viên thành công! Tài khoản đã được kích hoạt.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // Nếu lỗi (vd: trùng email) thì hiện lỗi ra
                    ModelState.AddModelError("", result);
                }
            }

            // Nếu dữ liệu không hợp lệ hoặc có lỗi, trả lại form để nhập lại
            return View(request);
        }

        // 4. SỬA (Tùy chọn: Nếu bạn muốn làm thêm chức năng sửa thông tin)
        public async Task<IActionResult> Edit(string id)
        {
            var nv = await _context.NhanVien.FindAsync(id);
            if (nv == null || nv.NhaXeId != NhaXeId) return NotFound();

            return View(nv);
        }

    }
}