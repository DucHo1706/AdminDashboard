using AdminDashboard.Helpers;
using AdminDashboard.Models;
using AdminDashboard.Services;
using AdminDashboard.TransportDBContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AdminDashboard.Controllers
{
    public class BookingController : Controller
    {
        private readonly Db27524Context _context;
        private readonly IVnpayService _vnpayService;
        private readonly IConfiguration _config;
        private readonly ILogger<BookingController> _logger; // Thêm Logger

        public BookingController(Db27524Context context,
                                 IVnpayService vnpayService,
                                 IConfiguration config,
                                 ILogger<BookingController> logger) // Thêm Logger vào hàm khởi tạo
        {
            _context = context;
            _vnpayService = vnpayService;
            _config = config;
            _logger = logger; // Gán Logger
        }

        [Authorize]
        [Authorize(Roles = "KhachHang")]
        public async Task<IActionResult> ChonGhe(string chuyenId)
        {
            if (string.IsNullOrEmpty(chuyenId))
            {
                return BadRequest("Khong co thong tin chuyen xe.");
            }

            var chuyenXe = await _context.ChuyenXe
                .Include(c => c.Xe)
                    .ThenInclude(x => x.DanhSachGhe)
                .Include(c => c.LoTrinh)
                    .ThenInclude(lt => lt.TramDiNavigation)
                .Include(c => c.LoTrinh)
                    .ThenInclude(lt => lt.TramToiNavigation)
                .Include(c => c.Images)
                .FirstOrDefaultAsync(c => c.ChuyenId == chuyenId);

            if (chuyenXe == null)
            {
                return NotFound("Chuyen xe khong ton tai.");
            }

            List<string> danhSachGheDaDat = new List<string>();

            try
            {
                var expiredOrders = await _context.DonHang
                    .Where(d => d.ChuyenId == chuyenId && d.TrangThaiThanhToan == "DangChoThanhToan" && d.ThoiGianHetHan < DateTime.Now)
                    .ToListAsync();

                if (expiredOrders.Any())
                {
                    foreach (var dh in expiredOrders)
                    {
                        var vesExpired = await _context.Ve.Where(v => v.DonHangId == dh.DonHangId).ToListAsync();
                        _context.Ve.RemoveRange(vesExpired);
                        dh.TrangThaiThanhToan = "Da huy"; // Sửa: Tiếng Việt không dấu
                    }
                    await _context.SaveChangesAsync();
                }

                danhSachGheDaDat = await _context.Ve
                    .Where(v => v.DonHang.ChuyenId == chuyenId &&
                                 (v.DonHang.TrangThaiThanhToan == "Da thanh toan" || // Sửa: Tiếng Việt không dấu
                                  (v.DonHang.TrangThaiThanhToan == "DangChoThanhToan" && v.DonHang.ThoiGianHetHan >= DateTime.Now)))
                    .Select(v => v.GheID)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Loi khi don dep hoac lay danh sach ghe da dat cho ChuyenId: {ChuyenId}", chuyenId);

                danhSachGheDaDat = await _context.Ve
                    .Where(v => v.DonHang.ChuyenId == chuyenId && v.DonHang.TrangThaiThanhToan == "Da thanh toan") // Sửa: Tiếng Việt không dấu
                    .Select(v => v.GheID)
                    .ToListAsync();
            }

            ViewBag.DanhSachGheDaDat = danhSachGheDaDat;
            return View(chuyenXe);
        }

        [HttpPost]
        [Authorize]
        [Authorize(Roles = "KhachHang")]
        public async Task<IActionResult> XacNhanBooking(string chuyenId, string danhSachGheId)
        {
            if (string.IsNullOrEmpty(chuyenId) || string.IsNullOrEmpty(danhSachGheId))
            {
                TempData["ErrorMessage"] = "Thong tin dat ve khong hop le, vui long thu lai.";
                return RedirectToAction("Index", "Home");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Challenge();
            }

            var cacGheIdDaChon = danhSachGheId.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

            var gheDaBiDat = await _context.Ve
                .FirstOrDefaultAsync(v => v.DonHang.ChuyenId == chuyenId && cacGheIdDaChon.Contains(v.GheID));

            if (gheDaBiDat != null)
            {
                var gheBiTrung = await _context.Ghe.FindAsync(gheDaBiDat.GheID);
                TempData["ErrorMessage"] = $"Rat tiec, ghe {gheBiTrung?.SoGhe} vua co nguoi khac dat. Vui long chon lai.";
                return RedirectToAction("ChonGhe", new { chuyenId = chuyenId });
            }

            var chuyenXe = await _context.ChuyenXe
                .Include(c => c.LoTrinh)
                .FirstOrDefaultAsync(c => c.ChuyenId == chuyenId);

            if (chuyenXe == null) return NotFound();

            decimal giaVe = chuyenXe.LoTrinh.GiaVeCoDinh ?? 0;
            decimal tongTien = cacGheIdDaChon.Count * giaVe;

            var donHang = new DonHang
            {
                DonHangId = Guid.NewGuid().ToString("N"),
                IDKhachHang = userId,
                ChuyenId = chuyenId,
                NgayDat = DateTime.Now,
                TongTien = tongTien,
                TrangThaiThanhToan = "DangChoThanhToan",
                ThoiGianHetHan = DateTime.Now.AddMinutes(15)
            };

            var danhSachVe = cacGheIdDaChon.Select(gheId => new Ve
            {
                VeId = Guid.NewGuid().ToString(),
                DonHangId = donHang.DonHangId,
                GheID = gheId,
                Gia = giaVe
            }).ToList();

            try
            {
                _context.DonHang.Add(donHang);
                _context.Ve.AddRange(danhSachVe);
                await _context.SaveChangesAsync();

                string paymentUrl = _vnpayService.CreatePaymentUrl(donHang, HttpContext);
                return Redirect(paymentUrl);
            }
            catch (Exception ex)
            {
                // **Ghi log lỗi ra Console Output**
                _logger.LogError(ex, "LỖI PHÁT SINH KHI TẠO URL VNPAY. DonHangId: {DonHangId}", donHang.DonHangId);

                TempData["ErrorMessage"] = "LỖI VNPay: " + ex.Message + " | Inner: " + ex.InnerException?.Message;
                return RedirectToAction("ChonGhe", new { chuyenId = chuyenId });
            }
        }

        public IActionResult VnpayReturn()
        {
            var vnpayData = new VnpayLibrary();
            foreach (var (key, value) in Request.Query)
            {
                vnpayData.AddResponseData(key, value);
            }

            var vnp_ResponseCode = vnpayData.GetResponseData("vnp_ResponseCode");
            var vnp_TxnRef = vnpayData.GetResponseData("vnp_TxnRef");
            var vnp_SecureHash = vnpayData.GetResponseData("vnp_SecureHash");

            var hashSecret = _config["Vnpay:HashSecret"];
            var isSignatureValid = vnpayData.ValidateSignature(vnp_SecureHash, hashSecret);

            if (isSignatureValid && vnp_ResponseCode == "00")
            {
                var donHang = _context.DonHang.Find(vnp_TxnRef);
                if (donHang != null && donHang.TrangThaiThanhToan == "DangChoThanhToan" && DateTime.Now <= donHang.ThoiGianHetHan)
                {
                    donHang.TrangThaiThanhToan = "Da thanh toan"; // Sửa: Tiếng Việt không dấu
                    _context.SaveChanges();
                }
                return RedirectToAction("BookingSuccess", new { id = vnp_TxnRef });
            }

            TempData["ErrorMessage"] = $"Thanh toan khong thanh cong. Ma loi VNPay: {vnp_ResponseCode}";
            var donHangFail = _context.DonHang.Find(vnp_TxnRef);
            return RedirectToAction("ChonGhe", new { chuyenId = donHangFail?.ChuyenId });
        }

        [Authorize]
        [Authorize(Roles = "KhachHang")]
        public async Task<IActionResult> BookingSuccess(string id)
        {
            if (id == null) return NotFound();

            var donHang = await _context.DonHang
                .Include(d => d.ChuyenXe).ThenInclude(cx => cx.LoTrinh).ThenInclude(lt => lt.TramDiNavigation)
                .Include(d => d.ChuyenXe).ThenInclude(cx => cx.LoTrinh).ThenInclude(lt => lt.TramToiNavigation)
                .Include(d => d.nguoiDung)
                .FirstOrDefaultAsync(d => d.DonHangId == id);

            if (donHang == null) return NotFound();

            ViewBag.DanhSachVe = await _context.Ve
                .Include(v => v.Ghe)
                .Where(v => v.DonHangId == id).ToListAsync();

            return View(donHang);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> PayAgain(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var donHang = await _context.DonHang.FirstOrDefaultAsync(d => d.DonHangId == id && d.IDKhachHang == userId);
            if (donHang == null) return NotFound();

            if (donHang.TrangThaiThanhToan != "DangChoThanhToan" && donHang.TrangThaiThanhToan != "Da huy") // Sửa
            {
                TempData["ErrorMessage"] = "Don hang da duoc thanh toan.";
                return RedirectToAction("BookingSuccess", new { id });
            }

            if (DateTime.Now > donHang.ThoiGianHetHan)
            {
                var ves = await _context.Ve.Where(v => v.DonHangId == donHang.DonHangId).ToListAsync();
                _context.Ve.RemoveRange(ves);
                donHang.TrangThaiThanhToan = "Da huy"; // Sửa
                await _context.SaveChangesAsync();
                TempData["ErrorMessage"] = "Don hang da het thoi gian thanh toan.";
                return RedirectToAction("BookingSuccess", new { id });
            }

            donHang.TrangThaiThanhToan = "DangChoThanhToan";
            await _context.SaveChangesAsync();

            var url = _vnpayService.CreatePaymentUrl(donHang, HttpContext);
            return Redirect(url);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var donHang = await _context.DonHang.FirstOrDefaultAsync(d => d.DonHangId == id && d.IDKhachHang == userId);
            if (donHang == null) return NotFound();

            if (donHang.TrangThaiThanhToan == "Da thanh toan") // Sửa
            {
                TempData["ErrorMessage"] = "Don hang da thanh toan khong the huy.";
                return RedirectToAction("BookingSuccess", new { id });
            }

            var ves = await _context.Ve.Where(v => v.DonHangId == donHang.DonHangId).ToListAsync();
            _context.Ve.RemoveRange(ves);
            donHang.TrangThaiThanhToan = "Da huy"; // Sửa
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Da huy don hang va giai phong ghe.";
            return RedirectToAction("PurchaseHistory", "Home_User");
        }

        [AllowAnonymous]
        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> PaymentIPN()
        {
            var vnpayData = new VnpayLibrary();
            foreach (var (key, value) in Request.Query)
            {
                vnpayData.AddResponseData(key, value);
            }

            var vnp_TxnRef = vnpayData.GetResponseData("vnp_TxnRef");
            var vnp_ResponseCode = vnpayData.GetResponseData("vnp_ResponseCode");
            var vnp_SecureHash = vnpayData.GetResponseData("vnp_SecureHash");
            var hashSecret = _config["Vnpay:HashSecret"];

            bool checkSignature = vnpayData.ValidateSignature(vnp_SecureHash, hashSecret);

            var response = new { RspCode = "", Message = "" };

            if (!checkSignature)
            {
                response = new { RspCode = "97", Message = "Invalid Signature" };
                return Json(response);
            }

            var donHang = await _context.DonHang.FindAsync(vnp_TxnRef);
            if (donHang == null)
            {
                response = new { RspCode = "01", Message = "Order not found" };
                return Json(response);
            }

            if (donHang.TrangThaiThanhToan != "DangChoThanhToan")
            {
                response = new { RspCode = "02", Message = "Order already confirmed" };
                return Json(response);
            }

            if (DateTime.Now > donHang.ThoiGianHetHan)
            {
                var ves = await _context.Ve.Where(v => v.DonHangId == donHang.DonHangId).ToListAsync();
                _context.Ve.RemoveRange(ves);
                donHang.TrangThaiThanhToan = "Da huy"; // Sửa
                await _context.SaveChangesAsync();
                response = new { RspCode = "98", Message = "Order expired" };
                return Json(response);
            }

            if (vnp_ResponseCode == "00")
            {
                donHang.TrangThaiThanhToan = "Da thanh toan"; // Sửa
                await _context.SaveChangesAsync();
                response = new { RspCode = "00", Message = "Confirm Success" };
            }
            else
            {
                response = new { RspCode = "99", Message = "Transaction Failed - Keep Pending" };
            }

            return Json(response);
        }
    }
}