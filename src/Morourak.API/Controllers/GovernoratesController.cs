using Microsoft.AspNetCore.Mvc;
using Morourak.Application.Interfaces.Services;

namespace Morourak.API.Controllers
{
    /// <summary>
    /// Controller for retrieving reference data about governorates and traffic units.
    /// </summary>
    [Route("api/v1/[controller]")]
    [Tags("Governorates & Traffic Units")]
    public class GovernoratesController : BaseApiController
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
        public async Task<IActionResult> GetGovernoratesAsync()
        {
            var governorates = await _governorateService.GetAllGovernoratesAsync();
            return Ok(new
            {
                isSuccess = true,
                message = (string?)null,
                errorCode = (string?)null,
                details = governorates
            });
        }

        /// <summary>
        /// Retrieves all traffic units belonging to a specific governorate.
        /// </summary>
        [HttpGet("{governorateId}/traffic-units")]
        public async Task<IActionResult> GetTrafficUnitsAsync(int governorateId)
        {
            var units = await _governorateService.GetTrafficUnitsByGovernorateAsync(governorateId);
            return Ok(new
            {
                isSuccess = true,
                message = (string?)null,
                errorCode = (string?)null,
                details = units
            });
        }
    }
}
