public enum OtpType
{
    Register,
    ResetPassword
}

public interface IOtpService
{
    Task<string> GenerateAndSendAsync(string email, OtpType type = OtpType.Register);
    Task<bool> ValidateAsync(string email, string code);
}
