using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Morourak.Application.Common;

namespace Morourak.Dashboard.Services
{
    public abstract class BaseApiService
    {
        protected readonly HttpClient HttpClient;
        protected readonly IHttpContextAccessor _httpContextAccessor;

        public BaseApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            HttpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        protected HttpRequestMessage CreateRequest(HttpMethod method, string endpoint)
        {
            var request = new HttpRequestMessage(method, endpoint);
            var token = _httpContextAccessor.HttpContext?.User.FindFirst("JWToken")?.Value;
            
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            return request;
        }

        protected async Task EnsureSuccessOrLogAsync(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[API ERROR] {response.StatusCode}: {content}");
            }
        }

        protected async Task<ApiResponse<T>> HandleResponseAsync<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                if (string.IsNullOrWhiteSpace(content)) return ApiResponse<T>.SuccessResult(default!, "Success");
                
                try
                {
                    return await response.Content.ReadFromJsonAsync<ApiResponse<T>>() 
                           ?? ApiResponse<T>.FailureResult("Failed to deserialize response.");
                }
                catch
                {
                    var data = JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return ApiResponse<T>.SuccessResult(data!, "Success");
                }
            }

            try 
            {
                var errorResult = JsonSerializer.Deserialize<ApiResponse<T>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return errorResult ?? ApiResponse<T>.FailureResult($"API Error ({response.StatusCode})");
            }
            catch 
            {
                return ApiResponse<T>.FailureResult($"API Error ({response.StatusCode}): {content}");
            }
        }
    }
}
