using Morourak.Application.DTOs.Auth;
using Morourak.Application.DTOs.Auth.Arabic;

namespace Morourak.Application.Interfaces.Services;

public interface IIdentityService
{
    Task<AuthResponseDto> LoginAsync(string mobileNumber, string password);
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> CreateTokenResponseAsync(string userId);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
}
