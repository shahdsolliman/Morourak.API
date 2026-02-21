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

        /// <summary>
        /// Get all violations for a driving license.
        /// </summary>
        [Authorize(Roles = "CITIZEN")]
        [HttpGet("driving-license/{licenseId}")]
        public async Task<IActionResult> GetDrivingLicenseViolations(int licenseId)
        {
            try
            {
                var result = await _service.GetViolationsByLicenseAsync(licenseId, LicenseType.Driving);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        #endregion

        #region Query — Vehicle License

        /// <summary>
        /// Get all violations for a vehicle license.
        /// </summary>
        [Authorize(Roles = "CITIZEN")]
        [HttpGet("vehicle-license/{licenseId}")]
        public async Task<IActionResult> GetVehicleLicenseViolations(int licenseId)
        {
            try
            {
                var result = await _service.GetViolationsByLicenseAsync(licenseId, LicenseType.Vehicle);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        #endregion

        #region Violation Details

        /// <summary>
        /// Get full details of a specific violation (for "View Details" button).
        /// </summary>
        [Authorize(Roles = "CITIZEN")]
        [HttpGet("{violationId}/details")]
        public async Task<IActionResult> GetViolationDetails(int violationId)
        {
            try
            {
                var result = await _service.GetViolationDetailsAsync(violationId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        #endregion

        #region Payment — Single

        /// <summary>
        /// Pay a single violation (supports partial payment).
        /// </summary>
        [Authorize(Roles = "CITIZEN")]
        [HttpPost("{violationId}/pay")]
        public async Task<IActionResult> PaySingleViolation(int violationId, [FromBody] PaySingleViolationDto dto)
        {
            try
            {
                var result = await _service.PaySingleViolationAsync(violationId, dto.Amount);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        #endregion

        #region Payment — Selected

        /// <summary>
        /// Pay multiple selected violations in full.
        /// </summary>
        [Authorize(Roles = "CITIZEN")]
        [HttpPost("pay-selected")]
        public async Task<IActionResult> PaySelectedViolations([FromBody] PaySelectedViolationsDto dto)
        {
            try
            {
                var result = await _service.PaySelectedViolationsAsync(dto.ViolationIds);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        #endregion

        #region Payment — All Driving

        /// <summary>
        /// Pay all unpaid violations for a driving license.
        /// </summary>
        [Authorize(Roles = "CITIZEN")]
        [HttpPost("driving-license/{licenseId}/pay-all")]
        public async Task<IActionResult> PayAllDrivingViolations(int licenseId)
        {
            try
            {
                var result = await _service.PayAllViolationsAsync(licenseId, LicenseType.Driving);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        #endregion

        #region Payment — All Vehicle

        /// <summary>
        /// Pay all unpaid violations for a vehicle license.
        /// </summary>
        [Authorize(Roles = "CITIZEN")]
        [HttpPost("vehicle-license/{licenseId}/pay-all")]
        public async Task<IActionResult> PayAllVehicleViolations(int licenseId)
        {
            try
            {
                var result = await _service.PayAllViolationsAsync(licenseId, LicenseType.Vehicle);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        #endregion
    }
}
