using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Morourak.Application.DTOs.Delivery;
using Morourak.Application.DTOs.DrivingLicenses;
using Morourak.Application.Interfaces;
using Morourak.Domain.Enums;
using Morourak.Application.Exceptions;
using Morourak.API.DTOs.DrivingLicenses;

namespace Morourak.API.Controllers
{
    /// <summary>
    /// Controller for managing driving license operations including applications, renewals, and replacements.
    /// </summary>
    [Route("api/v1/[controller]")]
    [Authorize(Roles = "CITIZEN")]
    [Tags("Driving Licenses")]
    public class DrivingLicenseController : BaseApiController
    {
        private readonly IDrivingLicenseService _service;

        public DrivingLicenseController(IDrivingLicenseService service)
        {
            _service = service;
        }

        // ================= UPLOAD INITIAL DOCUMENTS =================

        /// <summary>
        /// Uploads initial required documents for a new driving license application.
        /// </summary>
        [HttpPost("upload-documents")]
        public async Task<IActionResult> UploadDocuments([FromForm] UploadDrivingLicenseDocumentsApiDto apiDto)
        {
            if (apiDto == null)
                throw new ValidationException("لم يتم استلام أي بيانات.");

            var nationalId = NationalId;

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
            return Ok(new
            {
                isSuccess = true,
                message = "تم رفع المستندات بنجاح",
                errorCode = (string?)null,
                details = result
            });
        }

        // ================= FINALIZE LICENSE =================

        /// <summary>
        /// Finalizes a driving license application by providing delivery info after all steps are completed.
        /// </summary>
        [HttpPost("finalize/{requestNumber}")]
        public async Task<IActionResult> FinalizeLicense(string requestNumber, [FromBody] DeliveryInfoDto delivery)
        {
            var nationalId = NationalId;
            var result = await _service.FinalizeLicenseAsync(requestNumber, nationalId, delivery);
            return Ok(new
            {
                isSuccess = true,
                message = "تم إصدار الرخصة بنجاح",
                errorCode = (string?)null,
                details = result
            });
        }

        // ================= RENEW LICENSE =================

        /// <summary>
        /// Initiates a driving license renewal request.
        /// </summary>
        [HttpPost("renewal-request")]
        public async Task<IActionResult> SubmitRenewalRequest([FromForm] SubmitRenewalRequestApiDto apiDto)
        {
            var nationalId = NationalId;
            var dto = new SubmitRenewalRequestDto
            {
                NewCategory = apiDto.NewCategory,
            };

            var result = await _service.SubmitRenewalRequestAsync(nationalId, dto);
            return Ok(new
            {
                isSuccess = true,
                message = "تم تقديم طلب التجديد بنجاح",
                errorCode = (string?)null,
                details = result
            });
        }

        /// <summary>
        /// Finalizes a license renewal by providing delivery info.
        /// </summary>
        [HttpPost("finalize-renewal/{requestNumber}")]
        public async Task<IActionResult> FinalizeRenewal(string requestNumber, [FromBody] DeliveryInfoDto delivery)
        {
            var nationalId = NationalId;
            var result = await _service.FinalizeRenewalAsync(requestNumber, nationalId, delivery);
            return Ok(new
            {
                isSuccess = true,
                message = "تم تجديد الرخصة بنجاح",
                errorCode = (string?)null,
                details = result
            });
        }

        // ================= GET MY LICENSES =================

        /// <summary>
        /// Retrieves all driving licenses associated with the currently authenticated citizen.
        /// </summary>
        [HttpGet("my-licenses")]
        public async Task<IActionResult> GetMyLicenses()
        {
            var nationalId = NationalId;
            var licenses = await _service.GetAllLicensesByCitizenAsync(nationalId);
            return Ok(new
            {
                isSuccess = true,
                message = (string?)null,
                errorCode = (string?)null,
                details = licenses
            });
        }

        // ================= ISSUE REPLACEMENT =================

        /// <summary>
        /// Requests a replacement for a lost or damaged driving license.
        /// </summary>
        [HttpPost("issue-replacement/{drivingLicenseNumber}")]
        public async Task<IActionResult> IssueReplacement(string drivingLicenseNumber, [FromBody] IssueReplacementDrivingLicenseApiDto apiDto)
        {
            if (apiDto.ReplacementType != "Lost" && apiDto.ReplacementType != "Damaged")
                throw new ValidationException("ReplacementType must be 'Lost' or 'Damaged'.");

            var nationalId = NationalId;
            var result = await _service.IssueReplacementAsync(
                nationalId,
                drivingLicenseNumber,
                apiDto.ReplacementType,
                apiDto.Delivery
            );

            return Ok(new
            {
                isSuccess = true,
                message = "تم استخراج بدل الرخصة بنجاح",
                errorCode = (string?)null,
                details = result
            });
        }
    }
}
