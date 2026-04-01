namespace Morourak.Application.Interfaces.Services
{
    public enum OtpType
    {
        Register,
        ResetPassword
    }

    public interface IOtpService
    {
        Task<string> GenerateAndSendAsync(string identifier, OtpType type = OtpType.Register);
        Task<bool> ValidateAsync(string identifier, string code);
    }
}
