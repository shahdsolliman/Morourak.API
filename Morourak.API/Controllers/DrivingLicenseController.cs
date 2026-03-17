using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Morourak.API.Common;
using Morourak.Application.DTOs.Delivery;
using Morourak.Application.DTOs.DrivingLicenses;
using Morourak.Application.Interfaces;
using Morourak.Domain.Enums;
using Morourak.Application.Exceptions;

namespace Morourak.API.Controllers
{
    /// <summary>
    /// Controller for managing driving license operations including applications, renewals, and replacements.
    /// </summary>
    [ApiController]
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
        [ProducesResponseType(typeof(ApiResponseArabic), StatusCodes.Status200OK)]
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
            return Ok(ApiResponseArabic.Success(result, "تم رفع المستندات بنجاح"));
        }

        // ================= FINALIZE LICENSE =================

        /// <summary>
        /// Finalizes a driving license application by providing delivery info after all steps are completed.
        /// </summary>
        [HttpPost("finalize/{requestNumber}")]
        [ProducesResponseType(typeof(ApiResponseArabic), StatusCodes.Status200OK)]
        public async Task<IActionResult> FinalizeLicense(string requestNumber, [FromBody] DeliveryInfoDto delivery)
        {
            var nationalId = NationalId;
            var result = await _service.FinalizeLicenseAsync(requestNumber, nationalId, delivery);
            return Ok(ApiResponseArabic.Success(result, "تم إصدار الرخصة بنجاح"));
        }

        // ================= RENEW LICENSE =================

        /// <summary>
        /// Initiates a driving license renewal request.
        /// </summary>
        [HttpPost("renewal-request")]
        [ProducesResponseType(typeof(ApiResponseArabic), StatusCodes.Status200OK)]
        public async Task<IActionResult> SubmitRenewalRequest([FromForm] SubmitRenewalRequestApiDto apiDto)
        {
            var nationalId = NationalId;
            var dto = new SubmitRenewalRequestDto
            {
                NewCategory = apiDto.NewCategory,
            };

            var result = await _service.SubmitRenewalRequestAsync(nationalId, dto);
            return Ok(ApiResponseArabic.Success(result, "تم تقديم طلب التجديد بنجاح"));
        }

        /// <summary>
        /// Finalizes a license renewal by providing delivery info.
        /// </summary>
        [HttpPost("finalize-renewal/{requestNumber}")]
        [ProducesResponseType(typeof(ApiResponseArabic), StatusCodes.Status200OK)]
        public async Task<IActionResult> FinalizeRenewal(string requestNumber, [FromBody] DeliveryInfoDto delivery)
        {
            var nationalId = NationalId;
            var result = await _service.FinalizeRenewalAsync(requestNumber, nationalId, delivery);
            return Ok(ApiResponseArabic.Success(result, "تم تجديد الرخصة بنجاح"));
        }

        // ================= GET MY LICENSES =================

        /// <summary>
        /// Retrieves all driving licenses associated with the currently authenticated citizen.
        /// </summary>
        [HttpGet("my-licenses")]
        [ProducesResponseType(typeof(ApiResponseArabic), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyLicenses()
        {
            var nationalId = NationalId;
            var licenses = await _service.GetAllLicensesByCitizenAsync(nationalId);
            return Ok(ApiResponseArabic.Success(licenses));
        }

        // ================= ISSUE REPLACEMENT =================

        /// <summary>
        /// Requests a replacement for a lost or damaged driving license.
        /// </summary>
        [HttpPost("issue-replacement/{drivingLicenseNumber}")]
        [ProducesResponseType(typeof(ApiResponseArabic), StatusCodes.Status200OK)]
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

            return Ok(ApiResponseArabic.Success(result, "تم استخراج بدل الرخصة بنجاح"));
        }
    }
}
