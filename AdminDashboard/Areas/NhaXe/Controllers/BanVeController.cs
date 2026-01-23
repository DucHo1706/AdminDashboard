using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminDashboard.Areas.NhaXe.Controllers
{
    [Area("NhaXe")]
   // [Authorize(Roles = "ChuNhaXe, NhanVienBanVe")]
    public class BanVeController : Controller
    {
        // 1. Xem sơ đồ ghế để bán
        public IActionResult SoDoGhe(string chuyenId)
        {
            // Code hiển thị các ghế (Đã đặt / Trống)
            return View();
        }

        // 2. Đặt chỗ cho khách vãng lai (Bỏ qua thanh toán)
        [HttpPost]
        public IActionResult DatCho(string chuyenId, int soGhe, string tenKhach, string sdt)
        {
            // Lưu vào bảng VeXe:
            // TrangThai = DaThanhToan (Vì bán tại quầy là thu tiền mặt luôn)
            // KhachHangId = Null (Vì là khách vãng lai, chỉ lưu tên text)
            return Json(new { success = true });
        }
    }
}
