using System.Net;
using System.Net.Mail;
using System.Text;

namespace AdminDashboard.Services
{
    public interface IEmailService
    {
        Task<bool> SendOtpEmailAsync(string email, string otpCode);
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

        public async Task<bool> SendOtpEmailAsync(string email, string otpCode)
        {
            try
            {
                var smtpHost = _configuration["EmailSettings:SmtpHost"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
                var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
                var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
                var fromEmail = _configuration["EmailSettings:FromEmail"];
                var fromName = _configuration["EmailSettings:FromName"];

                using var client = new SmtpClient(smtpHost, smtpPort);
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                client.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName, Encoding.UTF8),
                    Subject = "Mã OTP đặt lại mật khẩu",
                    Body = CreateOtpEmailBody(otpCode),
                    IsBodyHtml = true,
                    BodyEncoding = Encoding.UTF8,
                    SubjectEncoding = Encoding.UTF8
                };

                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation($"OTP email sent successfully to {email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send OTP email to {email}");
                return false;
            }
        }

        private string CreateOtpEmailBody(string otpCode)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <title>Mã OTP đặt lại mật khẩu</title>
                </head>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #2c3e50; text-align: center;'>Đặt lại mật khẩu</h2>
                        <p>Xin chào,</p>
                        <p>Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản của mình.</p>
                        <p>Mã OTP của bạn là:</p>
                        <div style='background-color: #f8f9fa; border: 2px solid #dee2e6; border-radius: 8px; padding: 20px; text-align: center; margin: 20px 0;'>
                            <h1 style='color: #007bff; font-size: 32px; margin: 0; letter-spacing: 5px;'>{otpCode}</h1>
                        </div>
                        <p><strong>Lưu ý:</strong></p>
                        <ul>
                            <li>Mã OTP này có hiệu lực trong 10 phút</li>
                            <li>Chỉ sử dụng được một lần</li>
                            <li>Không chia sẻ mã này với bất kỳ ai</li>
                        </ul>
                        <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
                        <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>
                        <p style='font-size: 12px; color: #666; text-align: center;'>
                            Email này được gửi tự động từ hệ thống quản lý vận tải
                        </p>
                    </div>
                </body>
                </html>";
        }
    }
}
