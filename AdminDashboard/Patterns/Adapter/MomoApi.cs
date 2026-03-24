namespace AdminDashboard.Patterns.Adapter
{
    public class MomoApi
    {
        public string SendMomoRequest(string code, decimal money, string note, string callbackUrl)
        {
            return $"/Booking/MomoCheckout?id={code}";
        }
    }
}