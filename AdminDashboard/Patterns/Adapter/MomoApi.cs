namespace AdminDashboard.Patterns.Adapter
{
    public class MomoApi
    {
        public string SendMomoRequest(string code, decimal money, string note, string callbackUrl)
        {
            return $"https://test-payment.momo.vn/pay?code={code}&money={money}&note={note}&callback={callbackUrl}";
        }
    }
}