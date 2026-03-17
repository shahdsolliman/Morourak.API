using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Morourak.API.Common;
using Morourak.Application.Interfaces;
using Morourak.Application.Exceptions;

namespace Morourak.API.Controllers
{
    /// <summary>
    /// Controller for managing vehicle license operations including applications, renewals, and replacements.
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Tags("Vehicle Licenses")]
    public class VehicleLicenseController : BaseApiController
    {
        private readonly IVehicleLicenseService _service;

        public VehicleLicenseController(IVehicleLicenseService service)
        {
            _service = service;
        }


        // ================= UPLOAD DOCUMENTS =================

        /// <summary>
        /// Uploads required documents for a new vehicle license application.
        /// </summary>
        [Authorize(Roles = "CITIZEN")]
        [HttpPost("upload-documents")]
        [ProducesResponseType(typeof(ApiResponseArabic), StatusCodes.Status200OK)]
        public async Task<IActionResult> UploadDocuments([FromForm] UploadVehicleLicenseDocumentsApiDto apiDto)
        {
            var nationalId = NationalId;

            var dto = new Morourak.Application.DTOs.Vehicles.UploadVehicleDocsDto
            {
                VehicleType = apiDto.VehicleType,
                Brand = apiDto.Brand,
                Model = apiDto.Model,
                OwnershipProof = await ToByteArrayAsync(apiDto.OwnershipProof),
                VehicleDataCertificate = await ToByteArrayAsync(apiDto.VehicleDataCertificate),
                IdCard = await ToByteArrayAsync(apiDto.IdCard),
                InsuranceCertificate = await ToByteArrayAsync(apiDto.InsuranceCertificate),
                CustomClearance = apiDto.CustomClearance != null ? await ToByteArrayAsync(apiDto.CustomClearance) : null
            };

            var result = await _service.UploadInitialDocumentsAsync(nationalId, dto);
            return Ok(ApiResponseArabic.Success(result, "تم رفع المستندات بنجاح"));
        }

        // ================= FINALIZE LICENSE =================

        /// <summary>
        /// Finalizes a vehicle license application by providing delivery information.
        /// </summary>
        [Authorize(Roles = "CITIZEN")]
        [HttpPost("finalize/{requestNumber}")]
        [ProducesResponseType(typeof(ApiResponseArabic), StatusCodes.Status200OK)]
        public async Task<IActionResult> FinalizeLicense(string requestNumber, [FromBody] DeliveryInfoDto delivery)
        {
            var result = await _service.FinalizeLicenseAsync(requestNumber, nationalId, delivery);
            return Ok(ApiResponseArabic.Success(result, "تم إصدار الرخصة بنجاح"));
        }

        /// <summary>
        /// Finalizes a vehicle license renewal request by providing delivery info.
        /// </summary>
        [Authorize(Roles = "CITIZEN")]
        [HttpPost("finalize-renewal/{requestNumber}")]
        [ProducesResponseType(typeof(ApiResponseArabic), StatusCodes.Status200OK)]
        public async Task<IActionResult> FinalizeRenewal(string requestNumber, [FromBody] DeliveryInfoDto delivery)
        {
            var result = await _service.FinalizeRenewalAsync(requestNumber, nationalId, delivery);
            return Ok(ApiResponseArabic.Success(result, "تم تجديد الرخصة بنجاح"));
        }

        // ================= REPLACEMENT & RENEWAL =================

        /// <summary>
        /// Requests a replacement for a lost or damaged vehicle license.
        /// </summary>
        [Authorize(Roles = "CITIZEN")]
        [HttpPost("replacement/{licenseNumber}")]
        [ProducesResponseType(typeof(ApiResponseArabic), StatusCodes.Status200OK)]
        public async Task<IActionResult> IssueReplacement(string licenseNumber, [FromQuery] string type, [FromBody] DeliveryInfoDto delivery)
        {
            var result = await _service.IssueReplacementAsync(nationalId, licenseNumber, type, delivery);
            return Ok(ApiResponseArabic.Success(result, "تم استخراج بدل الرخصة بنجاح"));
        }

        /// <summary>
        /// Submits a request to renew an existing vehicle license.
        /// </summary>
        [Authorize(Roles = "CITIZEN")]
        [HttpPost("renew")]
        [ProducesResponseType(typeof(ApiResponseArabic), StatusCodes.Status200OK)]
        public async Task<IActionResult> Renew([FromForm] RenewVehicleLicenseApiDto apiDto)
        {
            var nationalId = NationalId;

            var dto = new Morourak.Application.DTOs.Vehicles.UploadVehicleDocsDto
            {
                VehicleLicenseNumber = apiDto.VehicleLicenseNumber,
            };

            var result = await _service.SubmitRenewalRequestAsync(nationalId, dto);
            return Ok(ApiResponseArabic.Success(result, "تم تقديم طلب التجديد بنجاح"));
        }

        // ================= GET MY LICENSES =================

        /// <summary>
        /// Retrieves all vehicle licenses owned by the currently authenticated citizen.
        /// </summary>
        [Authorize(Roles = "CITIZEN")]
        [HttpGet("my-licenses")]
        [ProducesResponseType(typeof(ApiResponseArabic), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyLicenses()
        {
            var licenses = await _service.GetAllLicensesByCitizenAsync(nationalId);
            return Ok(ApiResponseArabic.Success(licenses));
        }
    }
}
