using Microsoft.AspNetCore.Mvc;
using Morourak.API.Common;
using Morourak.Application.DTOs.Governorates.Arabic;
using Morourak.Application.Interfaces.Services;

namespace Morourak.API.Controllers
{
    /// <summary>
    /// Controller for retrieving reference data about governorates and traffic units.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Tags("Governorates & Traffic Units")]
    public class GovernoratesController : ControllerBase
    {
        private readonly IGovernorateService _governorateService;

        public GovernoratesController(IGovernorateService governorateService)
        {
            _governorateService = governorateService;
        }

        /// <summary>
        /// Retrieves all available governorates in Egypt.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponseArabic), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetGovernoratesAsync()
        {
            var governorates = await _governorateService.GetAllGovernoratesAsync();
            var arabicGovernorates = governorates.Select(g => new المحافظةDto
            {
                Id = g.Id,
                الاسم = g.Name
            });
            return Ok(ApiResponseArabic.Success(arabicGovernorates));
        }

        /// <summary>
        /// Retrieves all traffic units belonging to a specific governorate.
        /// </summary>
        [HttpGet("{governorateId}/traffic-units")]
        [ProducesResponseType(typeof(ApiResponseArabic), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseArabic), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTrafficUnitsAsync(int governorateId)
        {
            var units = await _governorateService.GetTrafficUnitsByGovernorateAsync(governorateId);
            var arabicUnits = units.Select(u => new وحدة_المرورDto
            {
                Id = u.Id,
                الاسم = u.Name,
                العنوان = u.Address,
                مواعيد_العمل = u.WorkingHours
            });
            return Ok(ApiResponseArabic.Success(arabicUnits));
        }
    }
}
