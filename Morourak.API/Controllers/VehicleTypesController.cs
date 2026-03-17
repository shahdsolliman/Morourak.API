using Microsoft.AspNetCore.Mvc;
using Morourak.Application.DTOs.Vehicles;
using Morourak.Application.Interfaces;
using Morourak.Application.Interfaces.Services;

namespace Morourak.API.Controllers
{
    /// <summary>
    /// Controller for retrieving supported vehicle types.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Tags("Vehicle Types")]
    public class VehicleTypesController : ControllerBase
    {
        private readonly IVehicleLicenseService _service;

        public VehicleTypesController(IVehicleLicenseService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves a list of all supported vehicle types in the system.
        /// </summary>
        /// <response code="200">A list of vehicle types retrieved successfully.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<VehicleTypeDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get()
        {
            var types = await _service.GetVehicleTypesAsync();
            return Ok(types);
        }
    }
}
