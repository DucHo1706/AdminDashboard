using AdminDashboard.Helpers;
using AdminDashboard.Models;
using AdminDashboard.Patterns;

namespace AdminDashboard.Services
{
    public interface IVnpayService
    {
        string CreatePaymentUrl(DonHang model, HttpContext context);
    }
    public class VnpayService : IVnpayService
    {
        private readonly IConfiguration _config;

        public VnpayService(IConfiguration config)
        {
            _config = config;
        }

        public string CreatePaymentUrl(DonHang donHang, HttpContext context)
        {
            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);

            var pay = new VnpayLibrary();
            var urlCallBack = VnPayConfiguration.Instance.ReturnUrl;
            var tmnCode = VnPayConfiguration.Instance.TmnCode;
            var hashSecret = VnPayConfiguration.Instance.HashSecret;

            pay.AddRequestData("vnp_Version", "2.1.0");
            pay.AddRequestData("vnp_Command", "pay");
            pay.AddRequestData("vnp_TmnCode", tmnCode);
            pay.AddRequestData("vnp_Amount", ((long)donHang.TongTien * 100).ToString()); 
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", "VND");
            pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
            pay.AddRequestData("vnp_Locale", "vn");
            pay.AddRequestData("vnp_OrderInfo", $"Thanh toan don hang {donHang.DonHangId}");
            pay.AddRequestData("vnp_OrderType", "other");
            pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
            pay.AddRequestData("vnp_TxnRef", donHang.DonHangId.ToString()); 

            // Tạo URL thanh toán
            var paymentUrl = pay.CreateRequestUrl(VnPayConfiguration.Instance.BaseUrl, hashSecret);

            return paymentUrl;
        }
    }
}