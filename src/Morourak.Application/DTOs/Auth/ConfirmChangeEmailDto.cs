namespace Morourak.Application.DTOs.Auth
{
    public class ConfirmChangeEmailDto
    {
        public string NewEmail { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
