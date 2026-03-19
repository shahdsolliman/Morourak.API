namespace Morourak.Application.Interfaces.Services
{
    public interface IMailService
    {
        Task SendAsync(
            string toEmail,
            string subject,
            string body);
    }
}
