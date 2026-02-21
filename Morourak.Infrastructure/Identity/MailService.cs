using Microsoft.Extensions.Options;
using Morourak.Application.Interfaces.Services;
using Morourak.Infrastructure.Settings;
using System.Net;
using System.Net.Mail;

namespace Morourak.Infrastructure.Identity
{
    public class MailService : IMailService
    {
        private readonly EmailSettings _settings;

        public MailService(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendAsync(string toEmail, string subject, string body)
        {
            try
            {
                using var client = new SmtpClient
                {
                    Host = _settings.SmtpServer,
                    Port = _settings.Port,
                    EnableSsl = _settings.EnableSSL,
                    Credentials = new NetworkCredential(
                        _settings.UserName,
                        _settings.Password)
                };

                using var message = new MailMessage
                {
                    From = new MailAddress(_settings.UserName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                message.To.Add(toEmail);

                await client.SendMailAsync(message);
            }
            catch (SmtpException ex)
            {
                throw new InvalidOperationException(
                    "Failed to send email. Please try again later.", ex);
            }
        }
    }
}