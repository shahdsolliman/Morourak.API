using Morourak.Application.DTOs.Auth;

namespace Morourak.Application.Interfaces.Services;

public interface IIdentityService
{
    Task<AuthResponseDto> LoginAsync(string mobileNumber, string password);
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> CreateTokenResponseAsync(string userId);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
}
