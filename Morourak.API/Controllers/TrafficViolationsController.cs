using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Morourak.API.Common;
using Morourak.Application.DTOs.Violations.Arabic;
using Morourak.Application.Interfaces.Services;
using Morourak.Domain.Enums.Violations;

namespace Morourak.API.Controllers
{
    /// <summary>
    /// Controller for querying and paying traffic violations for both driving and vehicle licenses.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Tags("Traffic Violations")]
    public class TrafficViolationsController : ControllerBase
    {
        private readonly ITrafficViolationService _service;

        public TrafficViolationsController(ITrafficViolationService service)
        {
            _service = service;
        }

        #region Helpers

        private مخالفةDto MapToArabic(Morourak.Application.DTOs.Violations.ViolationDto v)
        {
            return new مخالفةDto
            {
                Id = v.ViolationId,
                رقم_المخالفة = v.ViolationNumber,
                نوع_المخالفة = v.ViolationType,
                المادة_القانونية = v.LegalReference,
                الوصف = v.Description,
                الموقع = v.Location,
                تاريخ_ووقت_المخالفة = v.ViolationDateTime,
                قيمة_الغرامة = v.FineAmount,
                المبلغ_المدفوع = v.PaidAmount,
                المبلغ_المتبقي = v.RemainingAmount,
                الحالة = v.StatusAr,
                قابلة_للدفع = v.IsPayable
            };
        }

        #endregion

        #region Query — Driving License

        /// <summary>
        /// Retrieves all violations associated with a specific driving license.
        /// </summary>
        [Authorize(Roles = "CITIZEN")]
        [HttpGet("driving-license/{licenseNumber}")]
        [ProducesResponseType(typeof(ApiResponseArabic), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseArabic), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDrivingLicenseViolations(string licenseNumber)
        {
            var result = await _service.GetViolationsByLicenseNumberAsync(licenseNumber, LicenseType.Driving);
            var arabicResult = result.Violations.Select(MapToArabic);
            return Ok(ApiResponseArabic.Success(arabicResult));
        }

        #endregion

        #region Query — Vehicle License

        /// <summary>
        /// Retrieves all violations associated with a specific vehicle license.
        /// </summary>
        [Authorize(Roles = "CITIZEN")]
        [HttpGet("vehicle-license/{licenseNumber}")]
        [ProducesResponseType(typeof(ApiResponseArabic), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseArabic), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetVehicleLicenseViolations(string licenseNumber)
        {
            var result = await _service.GetViolationsByLicenseNumberAsync(licenseNumber, LicenseType.Vehicle);
            var arabicResult = result.Violations.Select(MapToArabic);
            return Ok(ApiResponseArabic.Success(arabicResult));
        }

        #endregion

        #region Violation Details

        /// <summary>
        /// Retrieves detailed violation information for a license of a specific type.
        /// </summary>
        [Authorize(Roles = "CITIZEN")]
        [HttpGet("license/{licenseNumber}/details")]
        [ProducesResponseType(typeof(ApiResponseArabic), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetViolationDetailsByLicenseNumber(
            string licenseNumber,
            [FromQuery] LicenseType licenseType)
        {
            var result = await _service.GetViolationsByLicenseNumberAsync(licenseNumber, licenseType);
            var arabicResult = result.Violations.Select(MapToArabic);
            return Ok(ApiResponseArabic.Success(arabicResult));
        }

        #endregion

    }
}
