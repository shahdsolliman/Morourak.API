using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Morourak.Dashboard.Services;
using Morourak.Application.DTOs.Admin;
using System.Linq;

namespace Morourak.Dashboard.Pages.Admin
{
    [Authorize(Roles = "ADMIN")]
    public class VehicleLicensesModel : PageModel
    {
        private readonly ISeedDataService _seedDataService;

        public VehicleLicensesModel(ISeedDataService seedDataService)
        {
            _seedDataService = seedDataService;
        }

        public IEnumerable<VehicleLicenseDto> Licenses { get; set; } = Enumerable.Empty<VehicleLicenseDto>();

        public async Task OnGetAsync()
        {
            try
            {
                Licenses = await _seedDataService.GetVehicleLicensesAsync();
            }
            catch { }
        }
    }
}
