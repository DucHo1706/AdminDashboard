using AdminDashboard.Models;

namespace AdminDashboard.Patterns.Adapter
{
    public interface IPaymentGateway
    {
        PaymentResult ProcessPayment(PaymentRequest request);
    }
}