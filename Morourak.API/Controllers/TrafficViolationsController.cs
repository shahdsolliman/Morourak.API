using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Morourak.Application.DTOs.Violations;
using Morourak.Application.Interfaces.Services;
using Morourak.Domain.Enums.Violations;

namespace Morourak.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrafficViolationsController : ControllerBase
    {
        private readonly ITrafficViolationService _service;

        public TrafficViolationsController(ITrafficViolationService service)
        {
            _service = service;
        }

        #region Query — Driving License

        [Authorize(Roles = "CITIZEN")]
        [HttpGet("driving-license/{licenseNumber}")]
        public async Task<IActionResult> GetDrivingLicenseViolations(string licenseNumber)
        {
            var result = await _service.GetViolationsByLicenseNumberAsync(licenseNumber, LicenseType.Driving);
            return Ok(result);
        }

        #endregion

        #region Query — Vehicle License

        [Authorize(Roles = "CITIZEN")]
        [HttpGet("vehicle-license/{licenseNumber}")]
        public async Task<IActionResult> GetVehicleLicenseViolations(string licenseNumber)
        {
            var result = await _service.GetViolationsByLicenseNumberAsync(licenseNumber, LicenseType.Vehicle);
            return Ok(result);
        }

        #endregion

        #region Violation Details

        [Authorize(Roles = "CITIZEN")]
        [HttpGet("license/{licenseNumber}/details")]
        public async Task<IActionResult> GetViolationDetailsByLicenseNumber(
    string licenseNumber,
    [FromQuery] LicenseType licenseType)
        {
            var result = await _service.GetViolationsByLicenseNumberAsync(licenseNumber, licenseType);
            return Ok(result);
        }

        #endregion

        #region Payment — Single

        [Authorize(Roles = "CITIZEN")]
        [HttpPost("{violationId}/pay")]
        public async Task<IActionResult> PaySingleViolation(int violationId, [FromBody] PaySingleViolationDto dto)
        {
            var result = await _service.PaySingleViolationAsync(violationId, dto.Amount);
            return Ok(result);
        }

        #endregion

        #region Payment — Selected

        [Authorize(Roles = "CITIZEN")]
        [HttpPost("pay-selected")]
        public async Task<IActionResult> PaySelectedViolations([FromBody] PaySelectedViolationsDto dto)
        {
            var result = await _service.PaySelectedViolationsAsync(dto.ViolationIds);
            return Ok(result);
        }

        #endregion

        #region Payment — All Driving

        [Authorize(Roles = "CITIZEN")]
        [HttpPost("driving-license/{licenseNumber}/pay-all")]
        public async Task<IActionResult> PayAllDrivingViolations(string licenseNumber)
        {
            var result = await _service.PayAllViolationsAsync(licenseNumber, LicenseType.Driving);
            return Ok(result);
        }

        #endregion

        #region Payment — All Vehicle

        [Authorize(Roles = "CITIZEN")]
        [HttpPost("vehicle-license/{licenseNumber}/pay-all")]
        public async Task<IActionResult> PayAllVehicleViolations(string licenseNumber)
        {
            var result = await _service.PayAllViolationsAsync(licenseNumber, LicenseType.Vehicle);
            return Ok(result);
        }

        #endregion
    }
}