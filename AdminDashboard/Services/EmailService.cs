using System.Net;
using System.Net.Mail;
<<<<<<< HEAD
=======
using System.Text;
>>>>>>> master

namespace AdminDashboard.Services
{
    public interface IEmailService
    {
<<<<<<< HEAD
        Task<bool> SendEmailAsync(string toEmail, string subject, string body);
        Task<bool> SendOtpEmailAsync(string toEmail, string otpCode);
=======
        Task<bool> SendOtpEmailAsync(string email, string otpCode);
>>>>>>> master
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

<<<<<<< HEAD
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
=======
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
>>>>>>> master

                using var client = new SmtpClient(smtpHost, smtpPort);
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                client.EnableSsl = true;

<<<<<<< HEAD
                var message = new MailMessage();
                message.From = new MailAddress(smtpUsername, "GoSix Transport System");
                message.To.Add(toEmail);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                await client.SendMailAsync(message);
                _logger.LogInformation($"Email sent successfully to {toEmail}");
=======
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
>>>>>>> master
                return true;
            }
            catch (Exception ex)
            {
<<<<<<< HEAD
                _logger.LogError(ex, $"Failed to send email to {toEmail}");
=======
                _logger.LogError(ex, $"Failed to send OTP email to {email}");
>>>>>>> master
                return false;
            }
        }

<<<<<<< HEAD
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
=======
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
>>>>>>> master
        }
    }
}
