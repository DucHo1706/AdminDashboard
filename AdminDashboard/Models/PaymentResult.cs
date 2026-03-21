namespace AdminDashboard.Models
{
    public class PaymentResult
    {
        public bool Success { get; set; }
        public string GatewayName { get; set; }
        public string PaymentUrl { get; set; }
        public string TransactionCode { get; set; }
        public string Message { get; set; }
    }
}