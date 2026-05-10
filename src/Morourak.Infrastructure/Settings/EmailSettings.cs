using System.ComponentModel.DataAnnotations;

namespace Morourak.Infrastructure.Settings
{
    public class EmailSettings
    {
        [Required]
        public string SmtpServer { get; set; } = default!;
        [Range(1, 65535)]
        public int Port { get; set; }
        public bool EnableSSL { get; set; }
        [Required, EmailAddress]
        public string UserName { get; set; } = default!;
        [Required]
        public string Password { get; set; } = default!;
    }
}