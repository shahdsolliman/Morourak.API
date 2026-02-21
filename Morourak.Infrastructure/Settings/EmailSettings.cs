namespace Morourak.Infrastructure.Settings
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = default!;
        public int Port { get; set; }
        public bool EnableSSL { get; set; }
        public string UserName { get; set; } = default!;
        public string Password { get; set; } = default!;
    }
}