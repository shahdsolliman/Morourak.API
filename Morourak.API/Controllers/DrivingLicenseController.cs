using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Morourak.API.DTOs.DrivingLicenses;
using Morourak.Application.DTOs.Delivery;
using Morourak.Application.DTOs.DrivingLicenses;
using Morourak.Application.Interfaces;

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
                throw new UnauthorizedAccessException("National ID not found in token.");
            return nationalId;
        }

        #endregion

        #region Upload Initial Documents

        [HttpPost("upload-documents")]
        public async Task<IActionResult> UploadDocuments([FromForm] UploadDrivingLicenseDocumentsApiDto apiDto)
        {
            if (apiDto == null)
                return BadRequest(new { Message = "No data received." });

            try
            {
                var nationalId = GetNationalId();

                var dto = new UploadDrivingLicenseDocumentsDto
                {
                    PersonalPhoto = await ToByteArrayAsync(apiDto.PersonalPhoto),
                    EducationalCertificate = await ToByteArrayAsync(apiDto.EducationalCertificate),
                    IdCard = await ToByteArrayAsync(apiDto.IdCard),
                    ResidenceProof = await ToByteArrayAsync(apiDto.ResidenceProof),
                    MedicalCertificate = await ToByteArrayAsync(apiDto.MedicalCertificate),
                    Category = apiDto.Category,
                    Governorate = apiDto.Governorate,
                    LicensingUnit = apiDto.LicensingUnit
                };

                var result = await _service.UploadInitialDocumentsAsync(nationalId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
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
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                if (ex.InnerException != null)
                    Console.WriteLine("Inner: " + ex.InnerException);
                return BadRequest(new { Message = ex.Message });
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
                    MedicalCertificate = await ToByteArrayAsync(apiDto.MedicalCertificate)
                };

                var result = await _service.SubmitRenewalRequestAsync(nationalId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
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
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
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
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        #endregion


        #region Issue Replacement

        [HttpPost("issue-replacement/{drivingLicenseNumber}")]
        public async Task<IActionResult> IssueReplacement(string drivingLicenseNumber, [FromBody] IssueReplacementDrivingLicenseApiDto apiDto)
        {
            if (apiDto.ReplacementType != "Lost" && apiDto.ReplacementType != "Damaged")
                return BadRequest(new { Message = "Replacement type must be 'Lost' or 'Damaged'." });

            try
            {
                var nationalId = GetNationalId();

                var result = await _service.IssueReplacementAsync(
                    nationalId,
                    drivingLicenseNumber,
                    apiDto.ReplacementType,
                    apiDto.Delivery
                );

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