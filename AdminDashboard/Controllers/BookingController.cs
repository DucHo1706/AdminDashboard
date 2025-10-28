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

        public BookingController(Db27524Context context, IVnpayService vnpayService, IConfiguration config)
        {
            _context = context;
            _vnpayService = vnpayService;
            _config = config;
        }

        // Action này được gọi khi người dùng nhấn "Chọn chuyến"
        [Authorize] // Chỉ người dùng đã đăng nhập mới được vào trang này
        public async Task<IActionResult> ChonGhe(string chuyenId)
        {
            if (string.IsNullOrEmpty(chuyenId))
            {
                return BadRequest("Không có thông tin chuyến xe.");
            }

            // Lấy thông tin chuyến xe, bao gồm cả Xe và Lộ trình
            var chuyenXe = await _context.ChuyenXe
                .Include(c => c.Xe)
                    .ThenInclude(x => x.DanhSachGhe) 
                .Include(c => c.LoTrinh)
                    .ThenInclude(lt => lt.TramDiNavigation)
                .Include(c => c.LoTrinh)
                    .ThenInclude(lt => lt.TramToiNavigation)
                .FirstOrDefaultAsync(c => c.ChuyenId == chuyenId);

            if (chuyenXe == null)
            {
                return NotFound("Chuyến xe không tồn tại.");
            }

            // Dọn dẹp các đơn đã hết hạn cho chuyến này: giải phóng ghế và đánh dấu hủy
            var expiredOrders = await _context.DonHang
                .Where(d => d.ChuyenId == chuyenId && d.TrangThaiThanhToan == "DangChoThanhToan" && d.ThoiGianHetHan < DateTime.Now)
                .ToListAsync();
            if (expiredOrders.Any())
            {
                foreach (var dh in expiredOrders)
                {
                    var vesExpired = await _context.Ve.Where(v => v.DonHangId == dh.DonHangId).ToListAsync();
                    _context.Ve.RemoveRange(vesExpired);
                    dh.TrangThaiThanhToan = "Đã hủy";
                }
                await _context.SaveChangesAsync();
            }

            // Lấy ID của những ghế đang bị giữ hợp lệ (chưa hết hạn) hoặc đã thanh toán
            var danhSachGheDaDat = await _context.Ve
                .Where(v => v.DonHang.ChuyenId == chuyenId &&
                            (v.DonHang.TrangThaiThanhToan == "Đã thanh toán" ||
                             (v.DonHang.TrangThaiThanhToan == "DangChoThanhToan" && v.DonHang.ThoiGianHetHan >= DateTime.Now)))
                .Select(v => v.GheID)
                .ToListAsync();

            // Đưa danh sách ghế đã đặt vào ViewBag
            ViewBag.DanhSachGheDaDat = danhSachGheDaDat;

            // Trả về View và truyền đối tượng chuyenXe làm model
            return View(chuyenXe);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> XacNhanBooking(string chuyenId, string danhSachGheId)
        {
            //  Kiểm tra thông tin đầu vào
            if (string.IsNullOrEmpty(chuyenId) || string.IsNullOrEmpty(danhSachGheId))
            {
                TempData["ErrorMessage"] = "Thông tin đặt vé không hợp lệ, vui lòng thử lại.";
                return RedirectToAction("Index", "Home"); // Chuyển về trang chủ
            }

            //  Lấy thông tin người dùng đang đăng nhập
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                // Yêu cầu đăng nhập nếu chưa có
                return Challenge();
            }

            var cacGheIdDaChon = danhSachGheId.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

            //  KIỂM TRA CHỐNG TRÙNG VÉ 
            // Kiểm tra xem có ai đã đặt mất ghế trong lúc mình đang chọn không
            var gheDaBiDat = await _context.Ve
                .FirstOrDefaultAsync(v => v.DonHang.ChuyenId == chuyenId && cacGheIdDaChon.Contains(v.GheID));

            if (gheDaBiDat != null)
            {
                var gheBiTrung = await _context.Ghe.FindAsync(gheDaBiDat.GheID);
                TempData["ErrorMessage"] = $"Rất tiếc, ghế {gheBiTrung?.SoGhe} vừa có người khác đặt. Vui lòng chọn lại.";
                return RedirectToAction("ChonGhe", new { chuyenId = chuyenId });
            }

            //  Lấy thông tin chuyến xe để tính giá
            var chuyenXe = await _context.ChuyenXe
                .Include(c => c.LoTrinh)
                .FirstOrDefaultAsync(c => c.ChuyenId == chuyenId);

            if (chuyenXe == null) return NotFound();

            decimal giaVe = chuyenXe.LoTrinh.GiaVeCoDinh ?? 0;
            decimal tongTien = cacGheIdDaChon.Count * giaVe;

            //  Tạo  DonHang và danh sách Ve
            var donHang = new DonHang
            {
                DonHangId = Guid.NewGuid().ToString("N"),
                IDKhachHang = userId,
                ChuyenId = chuyenId,
                NgayDat = DateTime.Now,
                TongTien = tongTien,
                TrangThaiThanhToan = "DangChoThanhToan", //  trạng thái
                ThoiGianHetHan = DateTime.Now.AddMinutes(15) // Giữ vé trong 15 phút
            };

            var danhSachVe = cacGheIdDaChon.Select(gheId => new Ve
            {
                VeId = Guid.NewGuid().ToString(),
                DonHangId = donHang.DonHangId,
                GheID = gheId,
                Gia = giaVe
            }).ToList();

            // Lưu tất cả vào cơ sở dữ liệu
            try
            {  
              

                _context.DonHang.Add(donHang);
                _context.Ve.AddRange(danhSachVe);
                await _context.SaveChangesAsync();

               //  tạo link VNPay và chuyển hướng người dùng
                string paymentUrl = _vnpayService.CreatePaymentUrl(donHang, HttpContext);
                return Redirect(paymentUrl);
            }
            catch (Exception ex)
            {
                // Nếu có lỗi, thông báo và quay lại trang chọn ghế
                TempData["ErrorMessage"] = "Lỗi hệ thống khi lưu vé: " + ex.Message;
                return RedirectToAction("ChonGhe", new { chuyenId = chuyenId });
            }
        }
        // Sửa lại Action VnpayReturn hiện tại của bạn

        public IActionResult VnpayReturn()
        {
            // Đọc các tham số trả về từ VNPay
            var vnpayData = new VnpayLibrary();
            foreach (var (key, value) in Request.Query)
            {
                vnpayData.AddResponseData(key, value);
            }

            var vnp_ResponseCode = vnpayData.GetResponseData("vnp_ResponseCode");
            var vnp_TxnRef = vnpayData.GetResponseData("vnp_TxnRef");
            var vnp_SecureHash = vnpayData.GetResponseData("vnp_SecureHash");

            // Fallback: nếu chữ ký hợp lệ và mã phản hồi 00, cập nhật trạng thái ngay
            var hashSecret = _config["Vnpay:HashSecret"];
            var isSignatureValid = vnpayData.ValidateSignature(vnp_SecureHash, hashSecret);

            if (isSignatureValid && vnp_ResponseCode == "00")
            {
                var donHang = _context.DonHang.Find(vnp_TxnRef);
                if (donHang != null && donHang.TrangThaiThanhToan == "DangChoThanhToan" && DateTime.Now <= donHang.ThoiGianHetHan)
                {
                    donHang.TrangThaiThanhToan = "Đã thanh toán";
                    _context.SaveChanges();
                }
                return RedirectToAction("BookingSuccess", new { id = vnp_TxnRef });
            }

            // Không thành công hoặc chữ ký không hợp lệ
            TempData["ErrorMessage"] = $"Thanh toán không thành công. Mã lỗi VNPay: {vnp_ResponseCode}";
            var donHangFail = _context.DonHang.Find(vnp_TxnRef);
            return RedirectToAction("ChonGhe", new { chuyenId = donHangFail?.ChuyenId });
        }
        [Authorize]
        public async Task<IActionResult> BookingSuccess(string id)
        {
            if (id == null) return NotFound();

            // Lấy thông tin đơn hàng vừa tạo để hiển thị
            var donHang = await _context.DonHang
                .Include(d => d.ChuyenXe).ThenInclude(cx => cx.LoTrinh).ThenInclude(lt => lt.TramDiNavigation)
                .Include(d => d.ChuyenXe).ThenInclude(cx => cx.LoTrinh).ThenInclude(lt => lt.TramToiNavigation)
                .Include(d => d.nguoiDung)
                .FirstOrDefaultAsync(d => d.DonHangId == id);

            if (donHang == null) return NotFound();

            // Lấy danh sách vé của đơn hàng này
            ViewBag.DanhSachVe = await _context.Ve
                .Include(v => v.Ghe) // Lấy thông tin số ghế
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

            if (donHang.TrangThaiThanhToan != "DangChoThanhToan" && donHang.TrangThaiThanhToan != "Đã hủy")
            {
                TempData["ErrorMessage"] = "Đơn hàng đã được thanh toán.";
                return RedirectToAction("BookingSuccess", new { id });
            }

            if (DateTime.Now > donHang.ThoiGianHetHan)
            {
                // Hết hạn thanh toán: hủy đơn và giải phóng ghế
                var ves = await _context.Ve.Where(v => v.DonHangId == donHang.DonHangId).ToListAsync();
                _context.Ve.RemoveRange(ves);
                donHang.TrangThaiThanhToan = "Đã hủy";
                await _context.SaveChangesAsync();
                TempData["ErrorMessage"] = "Đơn hàng đã hết thời gian thanh toán.";
                return RedirectToAction("BookingSuccess", new { id });
            }

            // Reset về chờ thanh toán để thanh toán lại
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

            if (donHang.TrangThaiThanhToan == "Đã thanh toán")
            {
                TempData["ErrorMessage"] = "Đơn hàng đã thanh toán không thể hủy.";
                return RedirectToAction("BookingSuccess", new { id });
            }

            // Hủy: xóa vé để giải phóng ghế và đánh dấu trạng thái
            var ves = await _context.Ve.Where(v => v.DonHangId == donHang.DonHangId).ToListAsync();
            _context.Ve.RemoveRange(ves);
            donHang.TrangThaiThanhToan = "Đã hủy";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã hủy đơn hàng và giải phóng ghế.";
            return RedirectToAction("PurchaseHistory", "Home_User");
        }

        // Thêm Action này vào BookingController.cs

        [AllowAnonymous] // IPN không yêu cầu đăng nhập
        [HttpGet]
        [HttpPost] // Cho phép cả GET và POST để tương thích cấu hình VNPay
        public async Task<IActionResult> PaymentIPN()
        {
            var vnpayData = new VnpayLibrary();
            // Dùng Request.Query vì VNPay thường gửi IPN qua GET
            foreach (var (key, value) in Request.Query)
            {
                vnpayData.AddResponseData(key, value);
            }

            var vnp_TxnRef = vnpayData.GetResponseData("vnp_TxnRef");
            var vnp_ResponseCode = vnpayData.GetResponseData("vnp_ResponseCode");
            var vnp_SecureHash = vnpayData.GetResponseData("vnp_SecureHash");
            var hashSecret = _config["Vnpay:HashSecret"];

            bool checkSignature = vnpayData.ValidateSignature(vnp_SecureHash, hashSecret);

            // Tạo đối tượng để trả về cho VNPay
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

            // Hết hạn thanh toán
            if (DateTime.Now > donHang.ThoiGianHetHan)
            {
                var ves = await _context.Ve.Where(v => v.DonHangId == donHang.DonHangId).ToListAsync();
                _context.Ve.RemoveRange(ves);
                donHang.TrangThaiThanhToan = "Đã hủy";
                await _context.SaveChangesAsync();
                response = new { RspCode = "98", Message = "Order expired" };
                return Json(response);
            }

            if (vnp_ResponseCode == "00")
            {
                donHang.TrangThaiThanhToan = "Đã thanh toán";
                await _context.SaveChangesAsync();
                response = new { RspCode = "00", Message = "Confirm Success" };
            }
            else
            {
                // Giao dịch thất bại hoặc người dùng hủy ở cổng thanh toán.
                // KHÔNG hủy đơn ngay; giữ trạng thái Chờ thanh toán để người dùng thanh toán lại
                // Việc hủy sẽ do hết hạn tự động hoặc người dùng chủ động hủy.
                response = new { RspCode = "99", Message = "Transaction Failed - Keep Pending" };
            }

            // Trả về JSON cho VNPay biết đã nhận được kết quả
            return Json(response);
        }
    }
}