using Microsoft.AspNetCore.Mvc;
using Morourak.Application.DTOs.Vehicles;
using Morourak.Application.Interfaces;
using Morourak.Application.Interfaces.Services;

namespace Morourak.API.Controllers
{
    /// <summary>
    /// Controller for retrieving supported vehicle types.
    /// </summary>
    [Route("api/v1/[controller]")]
    [Tags("Vehicle Types")]
    public class VehicleTypesController : BaseApiController
    {
        private readonly IVehicleLicenseService _service;

        public VehicleTypesController(IVehicleLicenseService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves a list of all supported vehicle types in the system.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var types = await _service.GetVehicleTypesAsync();
            return Ok(new
            {
                isSuccess = true,
                message = (string?)null,
                errorCode = (string?)null,
                details = types
            });
        }
    }
}
