namespace AdminDashboard.Models
{
    public class PaymentRequest
    {
        public string OrderId { get; set; }
        public decimal Amount { get; set; }
        public string OrderDescription { get; set; }
    }
}