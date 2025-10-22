using System.Net;
using System.Net.Mail;

namespace AdminDashboard.Services
{
    public interface IEmailService
    {
        Task<bool> SendOtpEmailAsync(string email, string otpCode, string userName);
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

        public async Task<bool> SendOtpEmailAsync(string email, string otpCode, string userName)
        {
            try
            {
                // Cấu hình SMTP (sử dụng Gmail SMTP)
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(
                        _configuration["EmailSettings:Email"], 
                        _configuration["EmailSettings:Password"]
                    ),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_configuration["EmailSettings:Email"], "GoSix - Hệ thống đặt vé"),
                    Subject = "Mã OTP khôi phục mật khẩu",
                    Body = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                            <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                                <h2 style='color: #4c6bff; text-align: center;'>GoSix - Khôi phục mật khẩu</h2>
                                
                                <p>Xin chào <strong>{userName}</strong>,</p>
                                
                                <p>Bạn đã yêu cầu khôi phục mật khẩu cho tài khoản của mình.</p>
                                
                                <div style='background-color: #f8f9fa; padding: 20px; border-radius: 8px; text-align: center; margin: 20px 0;'>
                                    <h3 style='color: #4c6bff; margin: 0;'>Mã OTP của bạn:</h3>
                                    <div style='font-size: 32px; font-weight: bold; color: #333; letter-spacing: 5px; margin: 10px 0;'>
                                        {otpCode}
                                    </div>
                                </div>
                                
                                <p><strong>Lưu ý:</strong></p>
                                <ul>
                                    <li>Mã OTP này có hiệu lực trong <strong>10 phút</strong></li>
                                    <li>Vui lòng không chia sẻ mã này với bất kỳ ai</li>
                                    <li>Nếu bạn không yêu cầu khôi phục mật khẩu, vui lòng bỏ qua email này</li>
                                </ul>
                                
                                <p>Trân trọng,<br>
                                <strong>Đội ngũ GoSix</strong></p>
                                
                                <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>
                                <p style='font-size: 12px; color: #666; text-align: center;'>
                                    Email này được gửi tự động, vui lòng không trả lời.
                                </p>
                            </div>
                        </body>
                        </html>",
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation($"OTP email sent successfully to {email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send OTP email to {email}");
                return false;
            }
        }
    }
}
