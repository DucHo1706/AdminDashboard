namespace AdminDashboard.Patterns
{
    public class VnPayConfiguration
    {
        private static VnPayConfiguration _instance;
        private static readonly object _lock = new object();

        public string TmnCode { get; private set; }
        public string HashSecret { get; private set; }
        public string BaseUrl { get; private set; }
        public string ReturnUrl { get; private set; }

        private VnPayConfiguration() { }

        public static VnPayConfiguration Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new VnPayConfiguration();
                    }
                    return _instance;
                }
            }
        }

        public void Initialize(IConfiguration config)
        {
            TmnCode = config["Vnpay:TmnCode"];
            HashSecret = config["Vnpay:HashSecret"];
            BaseUrl = config["Vnpay:BaseUrl"];
            ReturnUrl = config["Vnpay:ReturnUrl"];
        }
    }
}