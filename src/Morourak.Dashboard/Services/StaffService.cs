using Morourak.Application.Common;
using Morourak.Application.DTOs.Appointments;
using System.Net.Http.Json;
using System.Text.Json;

namespace Morourak.Dashboard.Services
{
    // تعريف الواجهة المطلوبة
    public interface IStaffService
    {
        Task<IEnumerable<AppointmentDto>> GetAppointmentsAsync(DateOnly? date = null);
        Task<AppointmentDto?> GetAppointmentByIdAsync(int id);
        Task<ApiResponse<object>> SubmitResultAsync(string requestNumber, bool passed, string? notes);
        Task<ApiResponse<object>> StartAppointmentAsync(int id);
    }

    public class StaffService : BaseApiService, IStaffService
    {
        private readonly JsonSerializerOptions _jsonOptions;

        public StaffService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor) 
            : base(httpClient, httpContextAccessor) 
        {
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<IEnumerable<AppointmentDto>> GetAppointmentsAsync(DateOnly? date = null)
        {
            var url = "staff"; 
            if (date.HasValue) url += $"?date={date.Value:yyyy-MM-dd}";

            var request = CreateRequest(HttpMethod.Get, url);
            var response = await HttpClient.SendAsync(request);
            await EnsureSuccessOrLogAsync(response);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content)) return Enumerable.Empty<AppointmentDto>();

                try
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<IEnumerable<AppointmentDto>>>(content, _jsonOptions);
                    if (apiResponse?.Details != null) return apiResponse.Details;
                }
                catch { }

                return ParseAppointments(content);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException("SessionExpired");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception(string.IsNullOrWhiteSpace(errorContent)
                ? $"API Error ({(int)response.StatusCode})"
                : $"API Error ({(int)response.StatusCode}): {errorContent}");
        }

        public async Task<AppointmentDto?> GetAppointmentByIdAsync(int id)
        {
            var request = CreateRequest(HttpMethod.Get, $"staff/appointment/{id}");
            var response = await HttpClient.SendAsync(request);
            await EnsureSuccessOrLogAsync(response);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await HandleResponseAsync<AppointmentDto>(response);
                return result.Details;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException("SessionExpired");
            }

            return null;
        }

        public async Task<ApiResponse<object>> StartAppointmentAsync(int id)
        {
            var request = CreateRequest(HttpMethod.Post, $"appointments/{id}/start");
            var response = await HttpClient.SendAsync(request);
            await EnsureSuccessOrLogAsync(response);
            return await HandleResponseAsync<object>(response);
        }

        public async Task<ApiResponse<object>> SubmitResultAsync(string requestNumber, bool passed, string? notes)
        {
            var request = CreateRequest(HttpMethod.Post, "staff/submit");
            request.Content = JsonContent.Create(new
            {
                RequestNumber = requestNumber,
                Passed = passed,
                Notes = notes
            });
            var response = await HttpClient.SendAsync(request);
            await EnsureSuccessOrLogAsync(response);
            return await HandleResponseAsync<object>(response);
        }

        // دالة مساعدة لمعالجة البيانات بمرونة
        private IEnumerable<AppointmentDto> ParseAppointments(string content)
        {
            try
            {
                var result = JsonSerializer.Deserialize<JsonElement>(content);
                // نبحث عن البيانات في details أو data لضمان التوافق
                if (result.ValueKind == JsonValueKind.Array)
                {
                    return JsonSerializer.Deserialize<IEnumerable<AppointmentDto>>(result.GetRawText(), _jsonOptions)
                           ?? Enumerable.Empty<AppointmentDto>();
                }

                if (TryGetPropertyIgnoreCase(result, "details", out var detailsCi))
                {
                    return JsonSerializer.Deserialize<IEnumerable<AppointmentDto>>(detailsCi.GetRawText(), _jsonOptions)
                           ?? Enumerable.Empty<AppointmentDto>();
                }
                if (TryGetPropertyIgnoreCase(result, "data", out var dataCi))
                {
                    return JsonSerializer.Deserialize<IEnumerable<AppointmentDto>>(dataCi.GetRawText(), _jsonOptions)
                           ?? Enumerable.Empty<AppointmentDto>();
                }

                if (result.TryGetProperty("details", out var detailsProp))
                {
                    return JsonSerializer.Deserialize<IEnumerable<AppointmentDto>>(detailsProp.GetRawText(), _jsonOptions) 
                           ?? Enumerable.Empty<AppointmentDto>();
                }
                if (result.TryGetProperty("data", out var dataProp))
                {
                    return JsonSerializer.Deserialize<IEnumerable<AppointmentDto>>(dataProp.GetRawText(), _jsonOptions) 
                           ?? Enumerable.Empty<AppointmentDto>();
                }
            }
            catch { }
            return Enumerable.Empty<AppointmentDto>();
        }

        private static bool TryGetPropertyIgnoreCase(JsonElement element, string propertyName, out JsonElement value)
        {
            value = default;
            if (element.ValueKind != JsonValueKind.Object) return false;

            foreach (var prop in element.EnumerateObject())
            {
                if (string.Equals(prop.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    value = prop.Value;
                    return true;
                }
            }

            return false;
        }
    }
}
