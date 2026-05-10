using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Morourak.Application.DTOs.Admin;
using Morourak.Dashboard.Services;
using System.Linq;

namespace Morourak.Dashboard.Pages.Admin
{
    [Authorize(Roles = "ADMIN")]
    public class IndexModel : PageModel
    {
        private readonly IAdminService _adminService;
        private readonly ISeedDataService _seedDataService;

        public IndexModel(IAdminService adminService, ISeedDataService seedDataService)
        {
            _adminService = adminService;
            _seedDataService = seedDataService;
        }

        public List<UserDto> Users { get; set; } = new();
        public int TotalCitizens { get; set; }
        public int TotalVehicleLicenses { get; set; }
        public int TotalDrivingLicenses { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                // Fetch Users
                var usersResult = await _adminService.GetUsersAsync(1, 10);
                if (usersResult != null && usersResult.IsSuccess)
                {
                    Users = usersResult.Details?.ToList() ?? new List<UserDto>();
                }
                else
                {
                    Users = new List<UserDto>();
                }

                // Fetch Stats Safely
                var citizens = await _seedDataService.GetCitizensAsync();
                TotalCitizens = citizens?.Count() ?? 0;

                var vLicenses = await _seedDataService.GetVehicleLicensesAsync();
                TotalVehicleLicenses = vLicenses?.Count() ?? 0;

                var dLicenses = await _seedDataService.GetDrivingLicensesAsync();
                TotalDrivingLicenses = dLicenses?.Count() ?? 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[INDEX ERROR] {ex.Message}");
                Users = new List<UserDto>();
            }
        }
    }
}
