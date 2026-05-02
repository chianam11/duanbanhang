using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ShopMVC.Services
{
    public interface IEmailService
    {
        Task SendOrderConfirmationAsync(string email, string orderNumber, decimal totalAmount);
        Task SendPasswordResetAsync(string email, string resetLink);
        Task SendWelcomeEmailAsync(string email, string userName);
        Task SendOrderStatusChangeAsync(string email, string orderNumber, string status);
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

        public async Task SendOrderConfirmationAsync(string email, string orderNumber, decimal totalAmount)
        {
            var subject = $"Xác nhận đơn hàng #{orderNumber}";
            var body = $@"
            <h2>Cảm ơn bạn đã đặt hàng!</h2>
            <p>Đơn hàng của bạn <strong>#{orderNumber}</strong> đã được tạo thành công.</p>
            <p><strong>Tổng tiền:</strong> {totalAmount:N0} VND</p>
            <p>Chúng tôi sẽ sớm xác nhận và gửi hàng cho bạn.</p>
            <p>Cảm ơn bạn!</p>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendPasswordResetAsync(string email, string resetLink)
        {
            var subject = "Đặt lại mật khẩu";
            var body = $@"
            <h2>Yêu cầu đặt lại mật khẩu</h2>
            <p>Bạn đã yêu cầu đặt lại mật khẩu.</p>
            <p><a href='{resetLink}'>Nhấn vào đây để đặt lại mật khẩu</a></p>
            <p>Link này sẽ hết hạn trong 30 phút.</p>
            <p>Nếu bạn không yêu cầu điều này, hãy bỏ qua email này.</p>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendWelcomeEmailAsync(string email, string userName)
        {
            var subject = "Chào mừng đến ShopMVC!";
            var body = $@"
            <h2>Chào mừng bạn, {userName}!</h2>
            <p>Tài khoản của bạn đã được tạo thành công.</p>
            <p>Bây giờ bạn có thể bắt đầu mua sắm trên ShopMVC.</p>
            <p>Nếu có bất kỳ câu hỏi nào, hãy liên hệ với chúng tôi.</p>
            <p>Cảm ơn!</p>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendOrderStatusChangeAsync(string email, string orderNumber, string status)
        {
            var statusText = GetStatusText(status);
            var subject = $"Đơn hàng #{orderNumber} - {statusText}";
            var body = $@"
            <h2>Cập nhật trạng thái đơn hàng</h2>
            <p>Đơn hàng <strong>#{orderNumber}</strong> của bạn hiện đang <strong>{statusText}</strong>.</p>
            <p>Bạn có thể theo dõi đơn hàng trên tài khoản của bạn.</p>
            <p>Cảm ơn bạn!</p>";

            await SendEmailAsync(email, subject, body);
        }

        private async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("SmtpSettings");
                var host = smtpSettings["Host"];
                var port = int.Parse(smtpSettings["Port"] ?? "587");
                var username = smtpSettings["Username"];
                var password = smtpSettings["Password"];
                var fromEmail = smtpSettings["FromEmail"] ?? username;

                if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(fromEmail))
                {
                    _logger.LogWarning("SMTP settings are incomplete. Skipping email send.");
                    return;
                }

                using (var client = new SmtpClient(host, port))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(username, password);

                    var mailMessage = new MailMessage(fromEmail, to)
                    {
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };

                    await client.SendMailAsync(mailMessage);
                    _logger.LogInformation("Email sent to {To}: {Subject}", to, subject);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {to}");
                // Don't throw - email failure shouldn't break order flow
            }
        }

        private string GetStatusText(string status)
        {
            return status switch
            {
                "Pending" => "Chờ xác nhận",
                "Confirmed" => "Đã xác nhận",
                "Shipped" => "Đang giao",
                "Delivered" => "Đã giao",
                "Cancelled" => "Đã hủy",
                _ => status
            };
        }
    }
}
