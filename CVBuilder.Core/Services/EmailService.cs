using System.Net.Mail;
using System.Net;
using CVBuilder.Core.Settings;
using Microsoft.Extensions.Options;

namespace CVBuilder.Core.Services
{
    public class EmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendResetPasswordEmail(string toEmail, string resetLink)
        {
            var smtpClient = new SmtpClient(_settings.SmtpServer)
            {
                Port = _settings.Port,
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(_settings.From),
                Subject = "Reset your CVBuilder password",
                Body = $"Click the link to reset your password: {resetLink}",
                IsBodyHtml = true
            };
            mail.To.Add(toEmail);
        }

    }
}
