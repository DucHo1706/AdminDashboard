using AdminDashboard.Models;
using AdminDashboard.TransportDBContext;
using Microsoft.EntityFrameworkCore;

namespace AdminDashboard.Services
{
    public interface IOtpService
    {
        Task<string> GenerateOtpAsync(string email, string purpose = "ResetPassword");
        Task<bool> VerifyOtpAsync(string email, string otpCode, string purpose = "ResetPassword");
        Task<bool> IsOtpValidAsync(string email, string purpose = "ResetPassword");
        Task CleanupExpiredOtpsAsync();
    }

    public class OtpService : IOtpService
    {
        private readonly Db27524Context _context;
        private readonly ILogger<OtpService> _logger;
        private readonly Random _random = new Random();

        public OtpService(Db27524Context context, ILogger<OtpService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string> GenerateOtpAsync(string email, string purpose = "ResetPassword")
        {
            try
            {
                // Xóa các OTP cũ của email này
                var existingOtps = await _context.OtpCodes
                    .Where(o => o.Email == email && o.Purpose == purpose)
                    .ToListAsync();

                _context.OtpCodes.RemoveRange(existingOtps);

                // Tạo mã OTP 6 chữ số
                var otpCode = _random.Next(100000, 999999).ToString();

                var otp = new OtpCode
                {
                    Email = email.ToLowerInvariant(),
                    Code = otpCode,
                    CreatedAt = DateTime.Now,
                    ExpiresAt = DateTime.Now.AddMinutes(10), // OTP có hiệu lực 10 phút
                    Purpose = purpose
                };

                _context.OtpCodes.Add(otp);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"OTP generated for {email}: {otpCode}");
                return otpCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to generate OTP for {email}");
                throw;
            }
        }

        public async Task<bool> VerifyOtpAsync(string email, string otpCode, string purpose = "ResetPassword")
        {
            try
            {
                var otp = await _context.OtpCodes
                    .FirstOrDefaultAsync(o => 
                        o.Email == email.ToLowerInvariant() && 
                        o.Code == otpCode && 
                        o.Purpose == purpose &&
                        !o.IsUsed &&
                        o.ExpiresAt > DateTime.Now);

                if (otp == null)
                {
                    _logger.LogWarning($"Invalid OTP attempt for {email}: {otpCode}");
                    return false;
                }

                // Đánh dấu OTP đã được sử dụng
                otp.IsUsed = true;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"OTP verified successfully for {email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to verify OTP for {email}");
                return false;
            }
        }

        public async Task<bool> IsOtpValidAsync(string email, string purpose = "ResetPassword")
        {
            try
            {
                var otp = await _context.OtpCodes
                    .FirstOrDefaultAsync(o => 
                        o.Email == email.ToLowerInvariant() && 
                        o.Purpose == purpose &&
                        !o.IsUsed &&
                        o.ExpiresAt > DateTime.Now);

                return otp != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to check OTP validity for {email}");
                return false;
            }
        }

        public async Task CleanupExpiredOtpsAsync()
        {
            try
            {
                var expiredOtps = await _context.OtpCodes
                    .Where(o => o.ExpiresAt < DateTime.Now)
                    .ToListAsync();

                if (expiredOtps.Any())
                {
                    _context.OtpCodes.RemoveRange(expiredOtps);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Cleaned up {expiredOtps.Count} expired OTPs");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cleanup expired OTPs");
            }
        }
    }
}

