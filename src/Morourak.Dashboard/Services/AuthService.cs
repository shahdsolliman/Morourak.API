using Morourak.Application.DTOs.Auth;
using Morourak.Application.Common;
using System.Net.Http.Json;

namespace Morourak.Dashboard.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> LoginAsync(LoginRequestDto loginRequest);
    }

    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto loginRequest)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("auth/login", loginRequest);
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
                
                if (response.IsSuccessStatusCode && result?.Details != null)
                {
                    result.Details.IsSuccess = true;
                    return result.Details;
                }
                
                return new AuthResponseDto 
                { 
                    IsSuccess = false, 
                    Message = result?.Message ?? $"Login failed ({response.StatusCode})" 
                };
            }
            catch (Exception ex)
            {
                return new AuthResponseDto { IsSuccess = false, Message = $"تعذر الاتصال بالخادم: {ex.Message}" };
            }
        }
    }
}
