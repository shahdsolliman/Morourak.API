using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Morourak.API.DTOs.DrivingLicenses;
using Morourak.Application.DTOs.Delivery;
using Morourak.Application.DTOs.DrivingLicenses;
using Morourak.Application.Interfaces;
using AppEx = Morourak.Application.Exceptions; 

namespace Morourak.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "CITIZEN")]
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
                throw new AppEx.ValidationException("National ID not found in token.", "AUTH_MISSING_NATIONAL_ID");
            return nationalId;
        }

        #endregion

        #region Upload Initial Documents

        [HttpPost("upload-documents")]
        public async Task<IActionResult> UploadDocuments([FromForm] UploadDrivingLicenseDocumentsApiDto apiDto)
        {
            if (apiDto == null)
                throw new AppEx.ValidationException("No data received.");

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

        [HttpPost("finalize/{requestNumber}")]
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

        [HttpPost("renewal-request")]
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

        [HttpPost("finalize-renewal/{requestNumber}")]
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

        #region Get All Licenses By Citizen

        [HttpGet("my-licenses")]
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

        [HttpPost("issue-replacement/{drivingLicenseNumber}")]
        public async Task<IActionResult> IssueReplacement(string drivingLicenseNumber, [FromBody] IssueReplacementDrivingLicenseApiDto apiDto)
        {
            try
            {
                if (apiDto.ReplacementType != "Lost" && apiDto.ReplacementType != "Damaged")
                    throw new AppEx.ValidationException("Replacement type must be 'Lost' or 'Damaged'.");

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