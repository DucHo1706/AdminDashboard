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
            if (request == null || string.IsNullOrEmpty(request.OrderId))
            {
                return new PaymentResult
                {
                    Success = false,
                    GatewayName = "MOMO",
                    PaymentUrl = string.Empty,
                    TransactionCode = string.Empty,
                    Message = "Du lieu thanh toan MOMO khong hop le"
                };
            }

            string paymentUrl = _momoApi.SendMomoRequest(
                request.OrderId,
                request.Amount,
                request.OrderDescription,
                "/Booking/MomoConfirm"
            );

            return new PaymentResult
            {
                Success = !string.IsNullOrEmpty(paymentUrl),
                GatewayName = "MOMO",
                PaymentUrl = paymentUrl,
                TransactionCode = request.OrderId,
                Message = "Tao URL thanh toan MOMO thanh cong"
            };
        }
    }
}