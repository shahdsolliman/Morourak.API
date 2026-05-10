using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Morourak.Dashboard.Services;
using Morourak.Application.DTOs.Admin;
using System.Linq;

namespace Morourak.Dashboard.Pages.Admin
{
    [Authorize(Roles = "ADMIN")]
    public class CitizensModel : PageModel
    {
        private readonly ISeedDataService _seedDataService;

        public CitizensModel(ISeedDataService seedDataService)
        {
            _seedDataService = seedDataService;
        }

        public IEnumerable<CitizenRegistryDto> Citizens { get; set; } = Enumerable.Empty<CitizenRegistryDto>();

        public async Task OnGetAsync()
        {
            try
            {
                Citizens = await _seedDataService.GetCitizensAsync();
            }
            catch { }
        }
    }
}
