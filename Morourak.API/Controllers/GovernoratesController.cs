using Microsoft.AspNetCore.Mvc;
using Morourak.API.Errors;
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
        /// <response code="200">A list of all governorates retrieved successfully.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<المحافظةDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetGovernoratesAsync()
        {
            var governorates = await _governorateService.GetAllGovernoratesAsync();
            var arabicGovernorates = governorates.Select(g => new المحافظةDto
            {
                Id = g.Id,
                الاسم = g.Name
            });
            return Ok(arabicGovernorates);
        }

        /// <summary>
        /// Retrieves all traffic units belonging to a specific governorate.
        /// </summary>
        /// <param name="governorateId">The unique identifier of the governorate.</param>
        /// <response code="200">A list of traffic units for the specified governorate.</response>
        /// <response code="404">Governorate not found.</response>
        [HttpGet("{governorateId}/traffic-units")]
        [ProducesResponseType(typeof(IEnumerable<وحدة_المرورDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
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
            return Ok(arabicUnits);
        }
    }
}
