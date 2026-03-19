using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Morourak.Application.Interfaces.Services;
using Morourak.Infrastructure.Settings;

namespace Morourak.Infrastructure.Identity
{
    public class MailService : IMailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<MailService> _logger;

        public MailService(IOptions<EmailSettings> options, ILogger<MailService> logger)
        {
            _settings = options.Value;
            _logger = logger;
        }

        public async Task SendAsync(string toEmail, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_settings.UserName));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };
            email.Body = builder.ToMessageBody();

            using var smtp = new MailKit.Net.Smtp.SmtpClient(); // <--- ?????? MailKit ???
            try
            {
                var socketOptions = _settings.Port == 465
                    ? SecureSocketOptions.SslOnConnect
                    : SecureSocketOptions.StartTls;

                await smtp.ConnectAsync(_settings.SmtpServer, _settings.Port, socketOptions);
                await smtp.AuthenticateAsync(_settings.UserName, _settings.Password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                throw new InvalidOperationException(
                    "Failed to send email. Please ensure your SMTP settings and App Password are correct.", ex);
            }
        }
    }
}