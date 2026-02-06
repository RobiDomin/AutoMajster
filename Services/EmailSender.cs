using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace WarsztatCar.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            // Pobieramy dane z appsettings.json
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var port = int.Parse(_configuration["EmailSettings:Port"]);
            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            var password = _configuration["EmailSettings:Password"];

            var client = new SmtpClient(smtpServer, port)
            {
                Credentials = new NetworkCredential(senderEmail, password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage(senderEmail, email, subject, message)
            {
                IsBodyHtml = true // Pozwala na HTML w mailu
            };

            await client.SendMailAsync(mailMessage);
        }
    }
}