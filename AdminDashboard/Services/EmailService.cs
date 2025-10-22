using System.Net;
using System.Net.Mail;

namespace AdminDashboard.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string body);
        Task<bool> SendOtpEmailAsync(string toEmail, string otpCode);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var smtpHost = _configuration["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
                var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
                var fromEmail = _configuration["EmailSettings:FromEmail"] ?? smtpUsername;
                if (string.IsNullOrEmpty(fromEmail))
                {
                    _logger.LogError("FromEmail is not configured");
                    return false;
                }

                if (string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
                {
                    _logger.LogError("Email configuration is missing");
                    return false;
                }

                using var client = new SmtpClient(smtpHost, smtpPort);
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                client.EnableSsl = true;

                var message = new MailMessage();
                message.From = new MailAddress(smtpUsername, "GoSix Transport System");
                message.To.Add(toEmail);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                await client.SendMailAsync(message);
                _logger.LogInformation($"Email sent successfully to {toEmail}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {toEmail}");
                return false;
            }
        }

        public async Task<bool> SendOtpEmailAsync(string toEmail, string otpCode)
        {
            var subject = "GoSix Transport OTP";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <div style='text-align: center; margin-bottom: 30px;'>
                        <h1 style='color: #2c5aa0; margin: 0;'>GoSix Transport</h1>
                        <p style='color: #666; margin: 5px 0;'>Đi nhanh, sống trọn</p>
                    </div>
                    <h2 style='color: #333; text-align: center;'>Đặt lại mật khẩu</h2>
                    <p>Xin chào,</p>
                    <p>Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản của mình trên hệ thống GoSix Transport.</p>
                    <p>Mã OTP của bạn là:</p>
                    <div style='background-color: #f5f5f5; padding: 20px; text-align: center; margin: 20px 0; border-radius: 8px;'>
                        <h1 style='color: #2c5aa0; font-size: 32px; margin: 0; letter-spacing: 5px;'>{otpCode}</h1>
                    </div>
                    <p><strong>Lưu ý:</strong></p>
                    <ul>
                        <li>Mã OTP này có hiệu lực trong 10 phút</li>
                        <li>Chỉ sử dụng một lần</li>
                        <li>Không chia sẻ mã này với bất kỳ ai</li>
                    </ul>
                    <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
                    <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>
                    <p style='color: #666; font-size: 12px; text-align: center;'>
                        Email này được gửi tự động từ hệ thống GoSix Transport<br>
                        © 2024 GoSix Transport. Tất cả quyền được bảo lưu.
                    </p>
                </div>";

            return await SendEmailAsync(toEmail, subject, body);
        }
    }
}
