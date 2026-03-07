using Microsoft.AspNetCore.Mvc;
using Morourak.Application.Interfaces;
using Morourak.Application.Interfaces.Services;

namespace Morourak.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehicleTypesController : ControllerBase
    {
        private readonly IVehicleLicenseService _service;

        public VehicleTypesController(IVehicleLicenseService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var types = await _service.GetVehicleTypesAsync();
            return Ok(types);
        }
    }
}
