using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace ConverterApi.Services
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string toEmail, string resetToken, string resetLink);
        Task SendVerificationEmailAsync(string toEmail, string verificationLink);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken, string resetLink)
        {
            var smtpServer = _config["Email:SmtpServer"];
            var smtpPort = int.Parse(_config["Email:SmtpPort"] ?? "587");
            var fromEmail = _config["Email:FromEmail"];
            var fromName = _config["Email:FromName"];
            var smtpUsername = _config["Email:SmtpUsername"];
            var smtpPassword = _config["Email:SmtpPassword"];

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(new MailboxAddress(toEmail, toEmail));
            message.Subject = "Şifre Sıfırlama Talebi";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                    <h2>Şifre Sıfırlama</h2>
                    <p>Merhaba,</p>
                    <p>Şifre sıfırlama talebiniz alınmıştır. Aşağıdaki kodu kullanarak şifrenizi sıfırlayabilirsiniz:</p>
                    <p><strong>Sıfırlama Kodu: {resetToken}</strong></p>
                    <p>Bu kod 1 saat geçerlidir.</p>
                    <p>İsteği yapmadıysanız bu mesajı görmezden gelebilirsiniz.</p>
                    <p>Saygılarımızla,<br/>Converter Ekibi</p>
                "
            };

            message.Body = bodyBuilder.ToMessageBody();

            try
            {
                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(smtpUsername, smtpPassword);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
            }
            catch
            {
                // For development, just log. In production, handle properly
                System.Console.WriteLine($"Email sending failed to {toEmail}");
            }
        }

        public async Task SendVerificationEmailAsync(string toEmail, string verificationLink)
        {
            var smtpServer = _config["Email:SmtpServer"];
            var smtpPort = int.Parse(_config["Email:SmtpPort"] ?? "587");
            var fromEmail = _config["Email:FromEmail"];
            var fromName = _config["Email:FromName"];
            var smtpUsername = _config["Email:SmtpUsername"];
            var smtpPassword = _config["Email:SmtpPassword"];

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(new MailboxAddress(toEmail, toEmail));
            message.Subject = "Email Doğrulama";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                    <h2>Email Doğrulama</h2>
                    <p>Merhaba,</p>
                    <p>Kaydınızı tamamlamak için lütfen aşağıdaki linke tıklayın:</p>
                    <p><a href='{verificationLink}'>Hesabımı Doğrula</a></p>
                    <p>veya bu linki tarayıcınıza yapıştırın: {verificationLink}</p>
                    <p>Saygılarımızla,<br/>Converter Ekibi</p>
                "
            };

            message.Body = bodyBuilder.ToMessageBody();

            try
            {
                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(smtpUsername, smtpPassword);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
            }
            catch(Exception ex)
            {
                System.Console.WriteLine($"Verification Email failed: {ex.Message}");
            }
        }
    }
}
