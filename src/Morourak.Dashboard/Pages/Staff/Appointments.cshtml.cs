using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Morourak.Application.DTOs.Appointments;
using Morourak.Dashboard.Services;

namespace Morourak.Dashboard.Pages.Staff
{
    [Authorize(Roles = "DOCTOR,EXAMINATOR,INSPECTOR")]
    public class AppointmentsModel : PageModel
    {
        private readonly IStaffService _staffService;

        public AppointmentsModel(IStaffService staffService)
        {
            _staffService = staffService;
        }

        public IEnumerable<AppointmentDto> Appointments { get; set; } = Enumerable.Empty<AppointmentDto>();
        
        [BindProperty(SupportsGet = true)]
        public DateOnly? SelectedDate { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                // Default to today if no date selected
                if (!SelectedDate.HasValue)
                {
                    SelectedDate = DateOnly.FromDateTime(DateTime.Now);
                }

                Appointments = await _staffService.GetAppointmentsAsync(SelectedDate);
            }
            catch { }
        }
    }
}
