using Microsoft.AspNetCore.Mvc;
using Morourak.Application.DTOs.Appointments;
using Morourak.Application.Interfaces.Services;

namespace Morourak.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArabicDataController : ControllerBase
    {
        private readonly IArabicDataService _arabicDataService;

        public ArabicDataController(IArabicDataService arabicDataService)
        {
            _arabicDataService = arabicDataService;
        }

        /// <summary>
        /// Retrieves appointment data fully in Arabic.
        /// </summary>
        /// <param name="id">Appointment ID.</param>
        /// <returns>Appointment and citizen details in Arabic.</returns>
        [HttpGet("appointment/{id}")]
        public async Task<ActionResult<ArabicAppointmentDto>> GetAppointmentArabic(int id)
        {
            var result = await _arabicDataService.GetArabicAppointmentByIdAsync(id);
            return Ok(result);
        }
    }
}
