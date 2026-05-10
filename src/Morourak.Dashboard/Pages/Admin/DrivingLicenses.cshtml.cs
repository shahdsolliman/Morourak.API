using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Morourak.Dashboard.Services;
using System.Linq;

namespace Morourak.Dashboard.Pages.Admin
{
    [Authorize(Roles = "ADMIN")]
    public class DrivingLicensesModel : PageModel
    {
        private readonly ISeedDataService _seedDataService;

        public DrivingLicensesModel(ISeedDataService seedDataService)
        {
            _seedDataService = seedDataService;
        }

        public IEnumerable<object> Licenses { get; set; } = Enumerable.Empty<object>();

        public async Task OnGetAsync()
        {
            try
            {
                Licenses = await _seedDataService.GetDrivingLicensesAsync();
            }
            catch { }
        }
    }
}
