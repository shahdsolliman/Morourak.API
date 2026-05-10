using Morourak.Application.DTOs.Common;
using Morourak.Application.DTOs.Admin;
using Morourak.Application.Common;
using System.Net.Http.Json;
using System.Linq;

namespace Morourak.Dashboard.Services
{
    public interface ISeedDataService
    {
        Task<IEnumerable<CitizenRegistryDto>> GetCitizensAsync();
        Task<IEnumerable<VehicleLicenseDto>> GetVehicleLicensesAsync();
        Task<IEnumerable<DrivingLicenseDto>> GetDrivingLicensesAsync();
    }

    public class SeedDataService : BaseApiService, ISeedDataService
    {
        private readonly System.Text.Json.JsonSerializerOptions _jsonOptions;

        public SeedDataService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor) 
            : base(httpClient, httpContextAccessor)
        {
            _jsonOptions = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<IEnumerable<CitizenRegistryDto>> GetCitizensAsync()
        {
            try
            {
                var request = CreateRequest(HttpMethod.Get, "AdminSeedData/citizens");
                var response = await HttpClient.SendAsync(request);
                await EnsureSuccessOrLogAsync(response);
                
                if (!response.IsSuccessStatusCode) return Enumerable.Empty<CitizenRegistryDto>();
                
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<CitizenRegistryDto>>>(_jsonOptions);
                return result?.Details ?? Enumerable.Empty<CitizenRegistryDto>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SEED ERROR] Citizens: {ex.Message}");
                return Enumerable.Empty<CitizenRegistryDto>(); 
            }
        }

        public async Task<IEnumerable<VehicleLicenseDto>> GetVehicleLicensesAsync()
        {
            try
            {
                var request = CreateRequest(HttpMethod.Get, "AdminSeedData/vehicle-licenses");
                var response = await HttpClient.SendAsync(request);
                await EnsureSuccessOrLogAsync(response);
                
                if (!response.IsSuccessStatusCode) return Enumerable.Empty<VehicleLicenseDto>();
                
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<VehicleLicenseDto>>>(_jsonOptions);
                return result?.Details ?? Enumerable.Empty<VehicleLicenseDto>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SEED ERROR] Vehicle: {ex.Message}");
                return Enumerable.Empty<VehicleLicenseDto>(); 
            }
        }

        public async Task<IEnumerable<DrivingLicenseDto>> GetDrivingLicensesAsync()
        {
            try
            {
                var request = CreateRequest(HttpMethod.Get, "AdminSeedData/driving-licenses");
                var response = await HttpClient.SendAsync(request);
                await EnsureSuccessOrLogAsync(response);
                
                if (!response.IsSuccessStatusCode) return Enumerable.Empty<DrivingLicenseDto>();
                
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<DrivingLicenseDto>>>(_jsonOptions);
                return result?.Details ?? Enumerable.Empty<DrivingLicenseDto>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SEED ERROR] Driving: {ex.Message}");
                return Enumerable.Empty<DrivingLicenseDto>(); 
            }
        }
    }
}
