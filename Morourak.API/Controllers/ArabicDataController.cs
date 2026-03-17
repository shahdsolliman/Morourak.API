using Microsoft.AspNetCore.Mvc;
using Morourak.API.Common;
using Morourak.Application.DTOs.Appointments;
using Morourak.Application.Interfaces.Services;

namespace Morourak.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ArabicDataController : BaseApiController
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
        public async Task<IActionResult> GetAppointmentArabic(int id)
        {
            var result = await _arabicDataService.GetArabicAppointmentByIdAsync(id);
            return Ok(ApiResponseArabic.Success(result));
        }
    }
}
