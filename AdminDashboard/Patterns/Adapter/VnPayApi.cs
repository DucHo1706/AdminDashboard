namespace AdminDashboard.Patterns.Adapter
{
    // Adaptee 1: giả lập thư viện / API cũ của VNPAY
    // Lớp này có cách gọi riêng, KHÔNG đúng theo IPaymentGateway
    public class VnPayApi
    {
        public string CreateVnPayUrl(string orderId, decimal amount, string description, string returnUrl)
        {
            return $"https://sandbox.vnpayment.vn/paymentv2/vpcpay.html?orderId={orderId}&amount={amount}&info={description}&returnUrl={returnUrl}";
        }
    }
}