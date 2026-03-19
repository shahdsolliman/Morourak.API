using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Morourak.Application.Interfaces;
using Morourak.Application.Exceptions;
using Morourak.Application.DTOs.Delivery;
using Morourak.API.DTOs.VehicleLicenses;

namespace Morourak.API.Controllers
{
    /// <summary>
    /// Controller for managing vehicle license operations including applications, renewals, and replacements.
    /// </summary>
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
        /// Finalizes a vehicle license application by providing delivery information.
        /// </summary>
        [Authorize(Roles = "CITIZEN")]
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

        /// <summary>
        /// Finalizes a vehicle license renewal request by providing delivery info.
        /// </summary>
        [Authorize(Roles = "CITIZEN")]
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

        // ================= REPLACEMENT & RENEWAL =================

        /// <summary>
        /// Requests a replacement for a lost or damaged vehicle license.
        /// </summary>
        [Authorize(Roles = "CITIZEN")]
        [HttpPost("replacement/{licenseNumber}")]
        public async Task<IActionResult> IssueReplacement(string licenseNumber, [FromQuery] string type, [FromBody] DeliveryInfoDto delivery)
        {
            var nationalId = NationalId;
            var result = await _service.IssueReplacementAsync(nationalId, licenseNumber, type, delivery);
            return Ok(new
            {
                isSuccess = true,
                message = "تم استخراج بدل الرخصة بنجاح",
                errorCode = (string?)null,
                details = result
            });
        }

        /// <summary>
        /// Submits a request to renew an existing vehicle license.
        /// </summary>
        [Authorize(Roles = "CITIZEN")]
        [HttpPost("renew")]
        public async Task<IActionResult> Renew([FromForm] RenewVehicleLicenseApiDto apiDto)
        {
            var nationalId = NationalId;

            var dto = new Morourak.Application.DTOs.Vehicles.UploadVehicleDocsDto
            {
                VehicleLicenseNumber = apiDto.VehicleLicenseNumber,
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

        // ================= GET MY LICENSES =================

        /// <summary>
        /// Retrieves all vehicle licenses owned by the currently authenticated citizen.
        /// </summary>
        [Authorize(Roles = "CITIZEN")]
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
    }
}
