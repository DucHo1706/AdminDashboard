using System.Collections.Concurrent;

namespace AdminDashboard.Services
{
    public interface IOtpService
    {
        Task<string> GenerateOtpAsync(string email, string purpose = "ResetPassword");
        Task<bool> VerifyOtpAsync(string email, string otpCode, string purpose = "ResetPassword", bool markAsUsed = false);
        Task<bool> IsOtpValidAsync(string email, string purpose = "ResetPassword");
        Task CleanupExpiredOtpsAsync();
    }

    public class OtpInfo
    {
        public string Email { get; set; }
        public string Code { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
        public string Purpose { get; set; }
    }

    public class OtpService : IOtpService
    {
        private readonly ILogger<OtpService> _logger;
        private readonly Random _random = new Random();
        
        // Sử dụng in-memory storage với ConcurrentDictionary để thread-safe
        private static readonly ConcurrentDictionary<string, OtpInfo> _otpStorage = new();

        public OtpService(ILogger<OtpService> logger)
        {
            _logger = logger;
            
            // Chạy cleanup mỗi 1 phút
            Task.Run(async () =>
            {
                while (true)
                {
                    await CleanupExpiredOtpsAsync();
                    await Task.Delay(TimeSpan.FromMinutes(1));
                }
            });
        }

        public Task<string> GenerateOtpAsync(string email, string purpose = "ResetPassword")
        {
            try
            {
                // Xóa các OTP cũ của email này
                var keysToRemove = _otpStorage.Keys
                    .Where(k => k.Contains(email.ToLowerInvariant()) && k.Contains(purpose))
                    .ToList();
                
                foreach (var key in keysToRemove)
                {
                    _otpStorage.TryRemove(key, out _);
                }

                // Tạo mã OTP 6 chữ số
                var otpCode = _random.Next(100000, 999999).ToString();
                var expiresAt = DateTime.Now.AddMinutes(3); // OTP có hiệu lực 3 phút

                var otpInfo = new OtpInfo
                {
                    Email = email.ToLowerInvariant(),
                    Code = otpCode,
                    CreatedAt = DateTime.Now,
                    ExpiresAt = expiresAt,
                    Purpose = purpose,
                    IsUsed = false
                };

                // Lưu vào in-memory storage
                var storageKey = $"{email.ToLowerInvariant()}_{purpose}_{DateTime.Now.Ticks}";
                _otpStorage.TryAdd(storageKey, otpInfo);

                _logger.LogInformation($"OTP generated for {email}: {otpCode}, expires at {expiresAt}");
                return Task.FromResult(otpCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to generate OTP for {email}");
                throw;
            }
        }

        public Task<bool> VerifyOtpAsync(string email, string otpCode, string purpose = "ResetPassword", bool markAsUsed = false)
        {
            try
            {
                var normalizedEmail = email.ToLowerInvariant();
                
                // Tìm OTP hợp lệ trong storage
                var otp = _otpStorage.Values
                    .FirstOrDefault(o => 
                        o.Email == normalizedEmail && 
                        o.Code == otpCode && 
                        o.Purpose == purpose &&
                        !o.IsUsed &&
                        o.ExpiresAt > DateTime.Now);

                if (otp == null)
                {
                    _logger.LogWarning($"Invalid OTP attempt for {email}: {otpCode}");
                    return Task.FromResult(false);
                }

                // Chỉ đánh dấu OTP đã được sử dụng nếu markAsUsed = true
                if (markAsUsed)
                {
                    otp.IsUsed = true;
                }

                _logger.LogInformation($"OTP verified successfully for {email}");
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to verify OTP for {email}");
                return Task.FromResult(false);
            }
        }

        public Task<bool> IsOtpValidAsync(string email, string purpose = "ResetPassword")
        {
            try
            {
                var normalizedEmail = email.ToLowerInvariant();
                
                var otp = _otpStorage.Values
                    .FirstOrDefault(o => 
                        o.Email == normalizedEmail && 
                        o.Purpose == purpose &&
                        !o.IsUsed &&
                        o.ExpiresAt > DateTime.Now);

                return Task.FromResult(otp != null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to check OTP validity for {email}");
                return Task.FromResult(false);
            }
        }

        public Task CleanupExpiredOtpsAsync()
        {
            try
            {
                var now = DateTime.Now;
                var expiredKeys = _otpStorage
                    .Where(kvp => kvp.Value.ExpiresAt < now)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var expiredKey in expiredKeys)
                {
                    _otpStorage.TryRemove(expiredKey, out _);
                }

                if (expiredKeys.Any())
                {
                    _logger.LogInformation($"Cleaned up {expiredKeys.Count} expired OTPs");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cleanup expired OTPs");
            }

            return Task.CompletedTask;
        }
    }
}

