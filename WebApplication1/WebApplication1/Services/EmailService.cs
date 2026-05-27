using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace WebApplication1.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task GuiEmailAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            var smtp     = _config["Email:SmtpHost"]     ?? "smtp.gmail.com";
            var port     = int.Parse(_config["Email:SmtpPort"] ?? "587");
            var user     = _config["Email:Username"]     ?? "";
            var pass     = _config["Email:Password"]     ?? "";
            var fromName = _config["Email:FromName"]     ?? "SmartParking SPMS";

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
                return; // Chưa cấu hình email — bỏ qua

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, user));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

            using var client = new SmtpClient();
            await client.ConnectAsync(smtp, port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(user, pass);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
