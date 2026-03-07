using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Morourak.API.DTOs.DrivingLicenses;
using Morourak.Application.DTOs.Delivery;
using Morourak.Application.DTOs.DrivingLicenses;
using Morourak.Application.DTOs.Licenses;
using Morourak.Application.Interfaces;
using AppEx = Morourak.Application.Exceptions;

namespace Morourak.API.Controllers
{
    /// <summary>
    /// Controller for managing driving license operations including applications, renewals, and replacements.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "CITIZEN")]
    [Tags("Driving Licenses")]
    public class DrivingLicenseController : ControllerBase
    {
        private readonly IDrivingLicenseService _service;

        public DrivingLicenseController(IDrivingLicenseService service)
        {
            _service = service;
        }

        #region Helpers

        private async Task<byte[]> ToByteArrayAsync(IFormFile file)
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            return ms.ToArray();
        }

        private string GetNationalId()
        {
            var nationalId = User.FindFirst("NationalId")?.Value;
            if (string.IsNullOrEmpty(nationalId))
                throw new AppEx.ValidationException("رقم الهوية غير موجود في رمز التحقق.", "AUTH_MISSING_NATIONAL_ID");
            return nationalId;
        }

        #endregion

        #region Upload Initial Documents

        /// <summary>
        /// Upload initial documents for a first-time driving license application.
        /// </summary>
        /// <param name="apiDto">The document data and license category info.</param>
        /// <returns>The created application details.</returns>
        [HttpPost("upload-documents")]
        [ProducesResponseType(typeof(DrivingLicenseApplicationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadDocuments([FromForm] UploadDrivingLicenseDocumentsApiDto apiDto)
        {
            if (apiDto == null)
                throw new AppEx.ValidationException("لم يتم استلام أي بيانات.");

            var nationalId = GetNationalId();

            var dto = new UploadDrivingLicenseDocumentsDto
            {
                PersonalPhoto = await ToByteArrayAsync(apiDto.PersonalPhoto),
                EducationalCertificate = await ToByteArrayAsync(apiDto.EducationalCertificate),
                IdCard = await ToByteArrayAsync(apiDto.IdCard),
                ResidenceProof = await ToByteArrayAsync(apiDto.ResidenceProof),
                Category = apiDto.Category,
                Governorate = apiDto.Governorate,
                LicensingUnit = apiDto.LicensingUnit
            };

            var result = await _service.UploadInitialDocumentsAsync(nationalId, dto);
            return Ok(result);
        }

        #endregion

        #region Finalize License

        /// <summary>
        /// Finalize the driving license process by providing delivery information.
        /// </summary>
        /// <param name="requestNumber">The service request number.</param>
        /// <param name="delivery">Delivery method and address details.</param>
        /// <returns>The issued driving license details.</returns>
        [HttpPost("finalize/{requestNumber}")]
        [ProducesResponseType(typeof(DrivingLicenseResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> FinalizeLicense(string requestNumber, [FromBody] DeliveryInfoDto delivery)
        {
            try
            {
                var nationalId = GetNationalId();
                var result = await _service.FinalizeLicenseAsync(requestNumber, nationalId, delivery);
                return Ok(result);
            }
            catch (AppEx.AppException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message,
                    code = ex.ErrorCode,
                    details = ex.Details
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected server error occurred." });
            }
        }

        #endregion

        #region Renew License

        /// <summary>
        /// Submit a request for driving license renewal.
        /// </summary>
        /// <param name="apiDto">Renewal request data (e.g., new category).</param>
        /// <returns>The renewal application details.</returns>
        [HttpPost("renewal-request")]
        [ProducesResponseType(typeof(DrivingLicenseApplicationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SubmitRenewalRequest([FromForm] SubmitRenewalRequestApiDto apiDto)
        {
            try
            {
                var nationalId = GetNationalId();
                var dto = new SubmitRenewalRequestDto
                {
                    NewCategory = apiDto.NewCategory,
                };

                var result = await _service.SubmitRenewalRequestAsync(nationalId, dto);
                return Ok(result);
            }
            catch (AppEx.AppException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message,
                    code = ex.ErrorCode,
                    details = ex.Details
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected server error occurred." });
            }
        }

        /// <summary>
        /// Finalize the driving license renewal by providing delivery information.
        /// </summary>
        /// <param name="requestNumber">The service request number.</param>
        /// <param name="delivery">Delivery method and address details.</param>
        /// <returns>The renewed driving license details.</returns>
        [HttpPost("finalize-renewal/{requestNumber}")]
        [ProducesResponseType(typeof(DrivingLicenseResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> FinalizeRenewal(string requestNumber, [FromBody] DeliveryInfoDto delivery)
        {
            try
            {
                var nationalId = GetNationalId();
                var result = await _service.FinalizeRenewalAsync(requestNumber, nationalId, delivery);
                return Ok(result);
            }
            catch (AppEx.AppException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message,
                    code = ex.ErrorCode,
                    details = ex.Details
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected server error occurred." });
            }
        }

        #endregion

        #region Get My Licenses

        /// <summary>
        /// Get all driving licenses belonging to the authenticated citizen.
        /// </summary>
        /// <returns>List of driving licenses.</returns>
        [HttpGet("my-licenses")]
        [ProducesResponseType(typeof(IEnumerable<DrivingLicenseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetMyLicenses()
        {
            try
            {
                var nationalId = GetNationalId();
                var licenses = await _service.GetAllLicensesByCitizenAsync(nationalId);

                if (!licenses.Any())
                    return Ok(new { Message = "No licenses found for this citizen." });

                return Ok(licenses);
            }
            catch (AppEx.AppException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message,
                    code = ex.ErrorCode,
                    details = ex.Details
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected server error occurred." });
            }
        }

        #endregion

        #region Issue Replacement

        /// <summary>
        /// Issue a replacement for a lost or damaged driving license.
        /// </summary>
        /// <param name="drivingLicenseNumber">The license number to replace.</param>
        /// <param name="apiDto">Replacement type (Lost/Damaged) and delivery info.</param>
        /// <returns>The new driving license details.</returns>
        [HttpPost("issue-replacement/{drivingLicenseNumber}")]
        [ProducesResponseType(typeof(DrivingLicenseResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> IssueReplacement(string drivingLicenseNumber, [FromBody] IssueReplacementDrivingLicenseApiDto apiDto)
        {
            try
            {
                if (apiDto.ReplacementType != "Lost" && apiDto.ReplacementType != "Damaged")
                    throw new AppEx.ValidationException("نوع البدل يجب أن يكون 'Lost' أو 'Damaged'.");

                var nationalId = GetNationalId();

                var result = await _service.IssueReplacementAsync(
                    nationalId,
                    drivingLicenseNumber,
                    apiDto.ReplacementType,
                    apiDto.Delivery
                );

                return Ok(result);
            }
            catch (AppEx.AppException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message,
                    code = ex.ErrorCode,
                    details = ex.Details
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected server error occurred." });
            }
        }

        #endregion
    }
}
