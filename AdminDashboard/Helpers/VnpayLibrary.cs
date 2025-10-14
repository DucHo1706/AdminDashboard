using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace AdminDashboard.Helpers
{
    public class VnpayLibrary
    {
        private readonly SortedList<string, string> _requestData = new SortedList<string, string>(new VnpayCompare());
        private readonly SortedList<string, string> _responseData = new SortedList<string, string>(new VnpayCompare());

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _requestData.Add(key, value);
            }
        }

        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _responseData.Add(key, value);
            }
        }

        public string GetResponseData(string key)
        {
            return _responseData.TryGetValue(key, out var retValue) ? retValue : string.Empty;
        }

        public string CreateRequestUrl(string baseUrl, string vnp_HashSecret)
        {
            var queryStringBuilder = new StringBuilder();
            var signDataBuilder = new StringBuilder();

            foreach (var (key, value) in _requestData)
            {
                // Tạo chuỗi signData (dữ liệu gốc, chưa mã hóa)
                signDataBuilder.Append(key + "=" + value + "&");
                // Tạo chuỗi query (dữ liệu đã mã hóa URL)
                queryStringBuilder.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
            }

            if (signDataBuilder.Length > 0)
            {
                signDataBuilder.Remove(signDataBuilder.Length - 1, 1);
            }
            if (queryStringBuilder.Length > 0)
            {
                queryStringBuilder.Remove(queryStringBuilder.Length - 1, 1);
            }

            var rawSignData = signDataBuilder.ToString();
            var vnp_SecureHash = HmacSha512(vnp_HashSecret, rawSignData); // Dùng HmacSha512

            queryStringBuilder.Append("&vnp_SecureHash=" + vnp_SecureHash);

            return baseUrl + "?" + queryStringBuilder.ToString();
        }

        public bool ValidateSignature(string inputHash, string secretKey)
        {
            var dataToHash = new SortedList<string, string>(new VnpayCompare());
            foreach (var (key, value) in _responseData)
            {
                if (key != "vnp_SecureHashType" && key != "vnp_SecureHash")
                {
                    dataToHash.Add(key, value);
                }
            }

            var signDataBuilder = new StringBuilder();
            foreach (var (key, value) in dataToHash)
            {
                signDataBuilder.Append(key + "=" + value + "&");
            }
            if (signDataBuilder.Length > 0)
            {
                signDataBuilder.Remove(signDataBuilder.Length - 1, 1);
            }

            var rspRaw = signDataBuilder.ToString();
            var myChecksum = HmacSha512(secretKey, rspRaw); // Dùng HmacSha512

            return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        // Đổi hàm thành non-static và sửa lại AppendFormat
        public string HmacSha512(string key, string inputData)
        {
            var hash = new StringBuilder();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var messageBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmacsha512 = new HMACSHA512(keyBytes))
            {
                var hashmessage = hmacsha512.ComputeHash(messageBytes);
                foreach (var b in hashmessage)
                {
                    hash.AppendFormat("{0:x2}", b);
                }
            }
            return hash.ToString();
        }

        public string GetIpAddress(HttpContext context)
        {
            var ipAddress = string.Empty;
            try
            {
                var remoteIpAddress = context.Connection.RemoteIpAddress;
                if (remoteIpAddress != null)
                {
                    if (remoteIpAddress.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        remoteIpAddress = Dns.GetHostEntry(remoteIpAddress).AddressList
                            .FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
                    }
                    if (remoteIpAddress != null) ipAddress = remoteIpAddress.ToString();
                }
            }
            catch (Exception)
            {
                ipAddress = "127.0.0.1";
            }
            return ipAddress;
        }
    }

    public class VnpayCompare : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            var vnpCompare = CompareInfo.GetCompareInfo("en-US");
            return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
        }
    }
}