using Morourak.Application.Common;
using Morourak.Application.DTOs.Admin;
using System.Net.Http.Json;

namespace Morourak.Dashboard.Services
{
    public interface IAdminService
    {
        Task<PagedApiResponse<UserDto>> GetUsersAsync(int page = 1, int pageSize = 10);
        Task<ApiResponse<UserDto>> CreateUserAsync(CreateUserDto dto);
        Task<ApiResponse<UserDto>> UpdateUserAsync(string id, UpdateUserDto dto);
        Task<ApiResponse<bool>> DeleteUserAsync(string id);
    }

    public class AdminService : BaseApiService, IAdminService
    {
        public AdminService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor) 
            : base(httpClient, httpContextAccessor)
        {
        }

        public async Task<PagedApiResponse<UserDto>> GetUsersAsync(int page = 1, int pageSize = 10)
        {
            var request = CreateRequest(HttpMethod.Get, $"AdminUsers?PageNumber={page}&PageSize={pageSize}");
            var response = await HttpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return System.Text.Json.JsonSerializer.Deserialize<PagedApiResponse<UserDto>>(content, options)
                       ?? new PagedApiResponse<UserDto>(Enumerable.Empty<UserDto>(), page, pageSize, 0, "No data returned", false);
            }

            return new PagedApiResponse<UserDto>(Enumerable.Empty<UserDto>(), page, pageSize, 0, $"API Error ({response.StatusCode}): {content}", false);
        }

        public async Task<ApiResponse<UserDto>> CreateUserAsync(CreateUserDto dto)
        {
            var request = CreateRequest(HttpMethod.Post, "AdminUsers");
            request.Content = JsonContent.Create(dto);
            var response = await HttpClient.SendAsync(request);
            return await HandleResponseAsync<UserDto>(response);
        }

        public async Task<ApiResponse<UserDto>> UpdateUserAsync(string id, UpdateUserDto dto)
        {
            var request = CreateRequest(HttpMethod.Put, $"AdminUsers/{id}");
            request.Content = JsonContent.Create(dto);
            var response = await HttpClient.SendAsync(request);
            return await HandleResponseAsync<UserDto>(response);
        }

        public async Task<ApiResponse<bool>> DeleteUserAsync(string id)
        {
            var request = CreateRequest(HttpMethod.Delete, $"AdminUsers/{id}");
            var response = await HttpClient.SendAsync(request);
            return await HandleResponseAsync<bool>(response);
        }
    }
}
