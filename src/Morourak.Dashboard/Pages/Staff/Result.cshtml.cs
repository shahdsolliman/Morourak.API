using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Morourak.Application.DTOs.Appointments;
using Morourak.Dashboard.Models;
using Morourak.Dashboard.Services;

namespace Morourak.Dashboard.Pages.Staff
{
    [Authorize(Roles = "DOCTOR,EXAMINATOR,INSPECTOR")]
    public class ResultModel : PageModel
    {
        private readonly IStaffService _staffService;

        public ResultModel(IStaffService staffService)
        {
            _staffService = staffService;
        }

        [BindProperty]
        public int AppointmentId { get; set; }

        [BindProperty]
        public SubmitResultDto Input { get; set; } = new();

        public AppointmentDto? Appointment { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                Appointment = await _staffService.GetAppointmentByIdAsync(id);
                if (Appointment == null)
                {
                    return RedirectToPage("/Staff/Appointments");
                }

                AppointmentId = id;
                Input.RequestNumber = Appointment.RequestNumberRelated ?? "";

                // Automatically start the appointment if it's in a pending state
                if (Appointment.Status == "محجوز" || Appointment.Status == "قيد الانتظار" || 
                    Appointment.Status == "Scheduled" || Appointment.Status == "Pending")
                {
                    await _staffService.StartAppointmentAsync(id);
                }

                return Page();
            }
            catch
            {
                return RedirectToPage("/Staff/Appointments");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Page();
                }

                var result = await _staffService.SubmitResultAsync(Input.RequestNumber, Input.Passed, Input.Notes);
                if (result != null && result.IsSuccess)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToPage("/Staff/Appointments");
                }

                ModelState.AddModelError(string.Empty, result?.Message ?? "فشل في تسجيل النتيجة.");
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "حدث خطأ غير متوقع أثناء الاتصال بالخادم.");
            }
            return Page();
        }
    }
}
