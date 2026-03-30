using AdminDashboard.Models.ViewModels;
using AdminDashboard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AdminDashboard.Areas.NhaXe.Controllers
{
    [Area("NhaXe")]
    [Authorize(Roles = "NhanVienBanVe,ChuNhaXe", AuthenticationSchemes = "CookieAuth")]
    public class BanVeController : Controller
    {
        private readonly IBanVeService _banVeService;

        public BanVeController(IBanVeService banVeService)
        {
            _banVeService = banVeService;
        }

        private string NhaXeId => User.FindFirst("NhaXeId")?.Value;
        private string TenNhanVien => User.Identity.Name;

        public async Task<IActionResult> Index(DateTime? ngayDi)
        {
            if (string.IsNullOrEmpty(NhaXeId)) return RedirectToAction("Login", "Auth", new { area = "" });
            var data = await _banVeService.GetChuyenXeBanVeAsync(NhaXeId, ngayDi ?? DateTime.Today);

            ViewBag.NgayDi = ngayDi ?? DateTime.Today;
            return View(data);
        }

        [HttpGet]
        public async Task<IActionResult> BanVe(string id)
        {
            if (string.IsNullOrEmpty(NhaXeId))
                return RedirectToAction("Login", "Auth", new { area = "" });

            var data = await _banVeService.GetSoDoGheAsync(id, NhaXeId);

            if (data == null) return NotFound();

            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> XacNhanBanVe([FromBody] DatVeTaiQuayRequest req)
        {
            if (string.IsNullOrEmpty(NhaXeId)) return Unauthorized();

            var result = await _banVeService.DatVeTaiQuayAsync(req, NhaXeId, TenNhanVien);

            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        public async Task<IActionResult> HuyVe(string chuyenId, string soGhe)
        {
            if (string.IsNullOrEmpty(NhaXeId)) return Unauthorized();

            var result = await _banVeService.HuyVeAsync(chuyenId, soGhe, NhaXeId);

            return Json(new { success = result.Success, message = result.Message });
        }
        // --- TRANG TRA CỨU VÉ ---
        [HttpGet]
        public IActionResult TraCuu()
        {
            return View();
        }

        // API Tìm vé (Gọi từ AJAX)
        [HttpGet]
        public async Task<IActionResult> TimKiemVe(string keyword)
        {
            if (string.IsNullOrEmpty(keyword)) return Json(new { success = false, message = "Vui lòng nhập thông tin." });

            var ve = await _banVeService.TraCuuVeAsync(keyword, NhaXeId);

            if (ve == null) return Json(new { success = false, message = "Không tìm thấy vé nào." });

            return Json(new { success = true, data = ve });
        }
        [HttpGet]
        public async Task<IActionResult> GetChuyenXeTheoNgay(DateTime ngayDi)
        {
            if (string.IsNullOrEmpty(NhaXeId)) return Json(new { success = false });

            // Gọi hàm cũ (nó sẽ lấy 7 ngày)
            var rawData = await _banVeService.GetChuyenXeBanVeAsync(NhaXeId, ngayDi);

            // LỌC LẠI: Chỉ lấy chính xác các chuyến chạy trong đúng ngày khách chọn
            var filteredData = rawData.Where(c => c.NgayDi.Date == ngayDi.Date).ToList();

            var result = filteredData.Select(c => new {
                chuyenId = c.ChuyenId,
                gioDi = c.GioDi.ToString(@"hh\:mm"),
                tuyenDuong = c.LoTrinh.TramDiNavigation.TenTram + " - " + c.LoTrinh.TramToiNavigation.TenTram,
                bienSo = c.Xe?.BienSoXe ?? "Chưa gán xe"
            });

            return Json(new { success = true, data = result });
        }
        // API Đổi ghế
        [HttpPost]
        public async Task<IActionResult> DoiGhe(string veId, string soGheMoi, string chuyenIdMoi)
        {
            var result = await _banVeService.DoiGheAsync(veId, soGheMoi, chuyenIdMoi, NhaXeId);
            return Json(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetSoDoGheJSON(string chuyenId)
        {
            if (string.IsNullOrEmpty(NhaXeId)) return Unauthorized();

            var data = await _banVeService.GetSoDoGheAsync(chuyenId, NhaXeId);

            if (data == null) return Json(new { success = false });

            return Json(new
            {
                success = true,
                tongSoGhe = data.TongSoGhe,
                veDaBan = data.VeDaBan,
                soGheTrong = data.SoGheTrong,
                soGheDaBan = data.SoGheDaBan
            });
        }
    }
}