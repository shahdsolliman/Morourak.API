using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Morourak.API.Common;
using Morourak.API.DTOs.VehicleLicenses;
using Morourak.Application.DTOs.Delivery;
using Morourak.Application.DTOs.Requests.Arabic;
using Morourak.Application.DTOs.Vehicles.Arabic;
using Morourak.Application.Interfaces;
using Morourak.Application.Exceptions;

namespace Morourak.API.Controllers
{
    /// <summary>
    /// Controller for managing vehicle license operations including applications, renewals, and replacements.
    /// </summary>
    [ApiController]
    [Tags("Vehicle Licenses")]
    public class VehicleLicenseController : BaseApiController
    {
        private readonly IVehicleLicenseService _service;

        public VehicleLicenseController(IVehicleLicenseService service)
        {
            _service = service;
        }

        private طلب_خدمةDto MapToArabic(Morourak.Application.DTOs.ServiceRequestDto request)
        {
            return new طلب_خدمةDto
            {
                رقم_الطلب = request.RequestNumber,
                الرقم_القومي = request.CitizenNationalId,
                نوع_الخدمة = request.ServiceType,
                الحالة = request.Status,
                تاريخ_التقديم = request.SubmittedAt,
                تاريخ_آخر_تحديث = request.LastUpdatedAt.GetValueOrDefault(),
                الرقم_المرجعي = request.ReferenceId.ToString(),
                الرسوم = new رسوم_طلب_الخدمةDto
                {
                    الرسوم_الأساسية = request.Fees.BaseFee,
                    رسوم_التوصيل = request.Fees.DeliveryFee,
                    المبلغ_الإجمالي = request.Fees.TotalAmount
                },
                التوصيل = new توصيل_طلب_الخدمةDto
                {
                    طريقة_التوصيل = request.Delivery.Method ?? string.Empty,
                    العنوان = request.Delivery.Address ?? string.Empty
                },
                الدفع = new دفع_طلب_الخدمةDto
                {
                    الحالة = request.Payment.Status,
                    رقم_العملية = request.Payment.TransactionId,
                    المبلغ = request.Payment.Amount ?? 0m,
                    الوقت = request.Payment.Timestamp
                }
            };
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
            
            var arabicResult = new طلب_رخصة_مركبةDto
            {
                Id = result.Id,
                نوع_المركبة = result.VehicleType,
                الماركة = result.Brand,
                الموديل = result.Model,
                الحالة = result.Status,
                رقم_الطلب = result.RequestNumber
            };

            return Ok(ApiResponseArabic.Success(arabicResult, "تم رفع المستندات بنجاح"));
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
            var nationalId = NationalId;
            var result = await _service.FinalizeLicenseAsync(requestNumber, nationalId, delivery);
            return Ok(ApiResponseArabic.Success(MapToArabic(result), "تم إصدار الرخصة بنجاح"));
        }

        /// <summary>
        /// Finalizes a vehicle license renewal request by providing delivery info.
        /// </summary>
        [Authorize(Roles = "CITIZEN")]
        [HttpPost("finalize-renewal/{requestNumber}")]
        [ProducesResponseType(typeof(ApiResponseArabic), StatusCodes.Status200OK)]
        public async Task<IActionResult> FinalizeRenewal(string requestNumber, [FromBody] DeliveryInfoDto delivery)
        {
            var nationalId = NationalId;
            var result = await _service.FinalizeRenewalAsync(requestNumber, nationalId, delivery);
            return Ok(ApiResponseArabic.Success(MapToArabic(result), "تم تجديد الرخصة بنجاح"));
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
            var nationalId = NationalId;
            var result = await _service.IssueReplacementAsync(nationalId, licenseNumber, type, delivery);
            return Ok(ApiResponseArabic.Success(MapToArabic(result), "تم استخراج بدل الرخصة بنجاح"));
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
            
            var arabicResult = new طلب_رخصة_مركبةDto
            {
                Id = result.Id,
                نوع_المركبة = result.VehicleType,
                الماركة = result.Brand,
                الموديل = result.Model,
                الحالة = result.Status,
                رقم_الطلب = result.RequestNumber
            };

            return Ok(ApiResponseArabic.Success(arabicResult, "تم تقديم طلب التجديد بنجاح"));
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
            var nationalId = NationalId;
            var licenses = await _service.GetAllLicensesByCitizenAsync(nationalId);
            
            var arabicLicenses = licenses.Select(l => new رخصة_مركبةDto
            {
                Id = l.Id,
                رقم_رخصة_المركبة = l.VehicleLicenseNumber,
                رقم_اللوحة = l.PlateNumber,
                نوع_المركبة = l.VehicleType,
                الماركة = l.Brand,
                الموديل = l.Model,
                الحالة = l.Status,
                تاريخ_الإصدار = l.IssueDate,
                تاريخ_الانتهاء = l.ExpiryDate,
                الرقم_القومي = l.CitizenNationalId
            });

            return Ok(ApiResponseArabic.Success(arabicLicenses));
        }
    }
}
