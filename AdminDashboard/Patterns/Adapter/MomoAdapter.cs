using AdminDashboard.Models;

namespace AdminDashboard.Patterns.Adapter
{
    public class MomoAdapter : IPaymentGateway
    {
        private readonly MomoApi _momoApi;

        public MomoAdapter()
        {
            _momoApi = new MomoApi();
        }

        public PaymentResult ProcessPayment(PaymentRequest request)
        {
            string paymentUrl = _momoApi.SendMomoRequest(
                request.OrderId,
                request.Amount,
                request.OrderDescription,
                "https://localhost:5001/Booking/PaymentCallback"
            );

            return new PaymentResult
            {
                Success = true,
                GatewayName = "MOMO",
                PaymentUrl = paymentUrl,
                TransactionCode = request.OrderId,
                Message = "Tao URL thanh toan MOMO thanh cong"
            };
        }
    }
}