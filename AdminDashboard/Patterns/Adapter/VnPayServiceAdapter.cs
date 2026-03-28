using AdminDashboard.Models;
using AdminDashboard.Services;
using Microsoft.AspNetCore.Http;

namespace AdminDashboard.Patterns.Adapter
{
    public class VnPayServiceAdapter : IPaymentGateway
    {
        private readonly IVnpayService _vnpayService;
        private readonly HttpContext _httpContext;

        public VnPayServiceAdapter(IVnpayService vnpayService, HttpContext httpContext)
        {
            _vnpayService = vnpayService;
            _httpContext = httpContext;
        }

        public PaymentResult ProcessPayment(PaymentRequest request)
        {
            var donHang = new DonHang
            {
                DonHangId = request.OrderId,
                TongTien = request.Amount
            };

            string paymentUrl = _vnpayService.CreatePaymentUrl(donHang, _httpContext);

            return new PaymentResult
            {
                Success = !string.IsNullOrEmpty(paymentUrl),
                GatewayName = "VNPAY",
                PaymentUrl = paymentUrl,
                TransactionCode = request.OrderId,
                Message = "Tao URL thanh toan VNPAY thanh cong"
            };
        }
    }
}