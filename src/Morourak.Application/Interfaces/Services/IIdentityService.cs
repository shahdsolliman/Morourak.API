using Morourak.Application.DTOs.Auth;

namespace Morourak.Application.Interfaces.Services;

public interface IIdentityService
{
    Task<AuthResponseDto> LoginAsync(string mobileNumber, string password);
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> CreateTokenResponseAsync(string userId);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
    Task<AuthResponseDto> ConfirmRegistrationAsync(string email, string code);
    Task<AuthResponseDto> RequestChangeEmailAsync(string userId, string newEmail);
    Task<AuthResponseDto> ConfirmChangeEmailAsync(string userId, string newEmail, string code);
    Task<AuthResponseDto> ForgotPasswordAsync(string email);
    Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordDto request);
}
