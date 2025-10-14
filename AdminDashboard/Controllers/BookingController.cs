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

            // Lấy ID của những ghế ĐÃ ĐƯỢC ĐẶT cho chuyến xe này
            var danhSachGheDaDat = await _context.Ve
                                    .Where(v => v.DonHang.ChuyenId == chuyenId)
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
        public async Task<IActionResult> VnpayReturn()
        {
            // Lấy các tham số trả về từ VNPay
            var vnpayData = new VnpayLibrary();
            foreach (string s in Request.Query.Keys)
            {
                vnpayData.AddResponseData(s, Request.Query[s]);
            }

            var vnp_TxnRef = vnpayData.GetResponseData("vnp_TxnRef"); // Mã đơn hàng
            var vnp_ResponseCode = vnpayData.GetResponseData("vnp_ResponseCode");
            var vnp_SecureHash = vnpayData.GetResponseData("vnp_SecureHash");
            var hashSecret = _config["Vnpay:HashSecret"]; // Lấy hash secret từ config

            // Xác thực chữ ký
            bool checkSignature = vnpayData.ValidateSignature(vnp_SecureHash, hashSecret);
            if (!checkSignature)
            {
                TempData["ErrorMessage"] = "Giao dịch không hợp lệ: Chữ ký không đúng.";
                // Có thể chuyển về trang lỗi hoặc trang chủ
                return RedirectToAction("Index", "Home");
            }

            var donHang = await _context.DonHang.FindAsync(vnp_TxnRef);
            if (donHang == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("Index", "Home");
            }

            // Nếu thanh toán thành công (mã 00)
            if (vnp_ResponseCode == "00")
            {
                // Cập nhật trạng thái đơn hàng
                donHang.TrangThaiThanhToan = "Đã thanh toán";
                await _context.SaveChangesAsync();

                // Chuyển hướng đến trang thành công
                return RedirectToAction("BookingSuccess", new { id = donHang.DonHangId });
            }
            else // Thanh toán thất bại
            {
                // Xóa đơn hàng và vé đã tạo để giải phóng ghế
                var veCanXoa = await _context.Ve.Where(v => v.DonHangId == donHang.DonHangId).ToListAsync();
                _context.Ve.RemoveRange(veCanXoa);
                _context.DonHang.Remove(donHang);
                await _context.SaveChangesAsync();

                TempData["ErrorMessage"] = $"Thanh toán VNPay không thành công. Mã lỗi: {vnp_ResponseCode}";
                // Trả người dùng về trang chọn ghế để thử lại
                return RedirectToAction("ChonGhe", new { chuyenId = donHang.ChuyenId });
            }
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

        // Thêm Action này vào BookingController.cs

        [AllowAnonymous] // Cho phép truy cập mà không cần đăng nhập để test
        public IActionResult TestVnpayHash()
        {
            // LẤY CHUỖI signData BẠN ĐÃ GỬI TÔI
            string signData = "vnp_Amount=500000&vnp_Command=pay&vnp_CreateDate=20251014222549&vnp_CurrCode=VND&vnp_IpAddr=26.233.138.208&vnp_Locale=vn&vnp_OrderInfo=Thanh+toan+don+hang+beb9b3bf302c499881b852ece918b127&vnp_OrderType=other&vnp_ReturnUrl=https%3A%2F%2Flocalhost%3A7063%2FBooking%2FVnpayReturn&vnp_TmnCode=W03JIUK4&vnp_TxnRef=beb9b3bf302c499881b852ece918b127&vnp_Version=2.1.0";

            // LẤY HASHSECRET TỪ APPSETTINGS.JSON
            var hashSecret = _config["Vnpay:HashSecret"];

            // TẠO CHỮ KÝ TỪ DỮ LIỆU TRÊN
            var vnpayLibrary = new VnpayLibrary();
            var secureHash = vnpayLibrary.HmacSha512(hashSecret, signData);

            // TRẢ VỀ KẾT QUẢ ĐỂ XEM
            return Content($"SignData: {signData}\nHashSecret: {hashSecret}\nGenerated Hash: {secureHash}");
        }
    }
}