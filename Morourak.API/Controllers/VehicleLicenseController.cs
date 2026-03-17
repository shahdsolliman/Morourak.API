using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Morourak.API.DTOs.VehicleLicenses;
using Morourak.API.DTOs.VehicleLicenses.Arabic;
using Morourak.API.Errors;
using Morourak.Application.DTOs.Delivery;
using Morourak.Application.DTOs.Delivery.Arabic;
using Morourak.Application.DTOs.Requests.Arabic;
using Morourak.Application.DTOs.Vehicles;
using Morourak.Application.DTOs.Vehicles.Arabic;
using Morourak.Application.Interfaces;
using AppEx = Morourak.Application.Exceptions;

namespace Morourak.API.Controllers
{
    /// <summary>
    /// Controller for managing vehicle license operations including applications, renewals, and replacements.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Tags("Vehicle Licenses")]
    public class VehicleLicenseController : ControllerBase
    {
        private readonly IVehicleLicenseService _service;

        public VehicleLicenseController(IVehicleLicenseService service)
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

        #endregion

        #region Upload Documents

        /// <summary>
        /// Uploads required documents for a new vehicle license application.
        /// </summary>
        /// <remarks>
        /// This step involves providing basic vehicle info (Type, Brand, Model) and uploading
        /// scans of ownership proof, ID card, insurance, and vehicle data certificate.
        /// </remarks>
        /// <param name="apiDto">Multipart form data containing vehicle details and document files.</param>
        /// <response code="200">Documents uploaded and application created successfully.</response>
        /// <response code="400">Invalid data, unsupported vehicle type, or missing files.</response>
        [Authorize(Roles = "CITIZEN")]
        [HttpPost("upload-documents")]
        [ProducesResponseType(typeof(طلب_رخصة_مركبةDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadDocuments([FromForm] رفع_مستندات_رخصة_المركبةApiDto apiDto)
        {
            var nationalId = GetNationalId();

            var dto = new UploadVehicleDocsDto
            {
                VehicleType = apiDto.نوع_المركبة,
                Brand = apiDto.الماركة,
                Model = apiDto.الموديل,
                OwnershipProof = await ToByteArrayAsync(apiDto.إثبات_الملكية),
                VehicleDataCertificate = await ToByteArrayAsync(apiDto.شهادة_بيانات_المركبة),
                IdCard = await ToByteArrayAsync(apiDto.بطاقة_الرقم_القومي),
                InsuranceCertificate = await ToByteArrayAsync(apiDto.شهادة_التأمين),
                CustomClearance = apiDto.الشهادة_الجمركية != null ? await ToByteArrayAsync(apiDto.الشهادة_الجمركية) : null
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

            return Ok(arabicResult);
        }

        #endregion

        #region Finalize License

        /// <summary>
        /// Finalizes a vehicle license application by providing delivery information.
        /// </summary>
        /// <param name="requestNumber">The tracking number of the service request.</param>
        /// <param name="delivery">Delivery method and address details.</param>
        /// <response code="200">Vehicle license finalized and issued successfully.</response>
        /// <response code="404">Request not found or not in finalizable state.</response>
        [Authorize(Roles = "CITIZEN")]
        [HttpPost("finalize/{requestNumber}")]
        [ProducesResponseType(typeof(طلب_خدمةDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> FinalizeLicense(string requestNumber, [FromBody] معلومات_التوصيلDto delivery)
        {
            var nationalId = GetNationalId();
            
            var englishDelivery = new DeliveryInfoDto
            {
                Method = delivery.طريقة_التوصيل,
                Address = delivery.العنوان != null ? new AddressDto
                {
                    Governorate = delivery.العنوان.المحافظة,
                    City = delivery.العنوان.المدينة,
                    Details = delivery.العنوان.التفاصيل
                } : null
            };

            var result = await _service.FinalizeLicenseAsync(requestNumber, nationalId, englishDelivery);
            return Ok(MapToArabic(result));
        }

        /// <summary>
        /// Finalizes a vehicle license renewal request by providing delivery info.
        /// </summary>
        /// <param name="requestNumber">The tracking number of the renewal request.</param>
        /// <param name="delivery">Delivery details.</param>
        /// <response code="200">Renewal finalized and license updated.</response>
        /// <response code="404">Renewal request not found.</response>
        [Authorize(Roles = "CITIZEN")]
        [HttpPost("finalize-renewal/{requestNumber}")]
        [ProducesResponseType(typeof(طلب_خدمةDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> FinalizeRenewal(string requestNumber, [FromBody] معلومات_التوصيلDto delivery)
        {
            var nationalId = GetNationalId();

            var englishDelivery = new DeliveryInfoDto
            {
                Method = delivery.طريقة_التوصيل,
                Address = delivery.العنوان != null ? new AddressDto
                {
                    Governorate = delivery.العنوان.المحافظة,
                    City = delivery.العنوان.المدينة,
                    Details = delivery.العنوان.التفاصيل
                } : null
            };

            var result = await _service.FinalizeRenewalAsync(requestNumber, nationalId, englishDelivery);
            return Ok(MapToArabic(result));
        }

        #endregion

        #region Replacement & Renewal

        /// <summary>
        /// Requests a replacement for a lost or damaged vehicle license.
        /// </summary>
        /// <param name="licenseNumber">The license number to be replaced.</param>
        /// <param name="type">The replacement type: 'Lost' or 'Damaged'.</param>
        /// <param name="delivery">Delivery details for the new license.</param>
        /// <response code="200">Replacement request processed and license issued.</response>
        /// <response code="404">Original vehicle license not found.</response>
        [Authorize(Roles = "CITIZEN")]
        [HttpPost("replacement/{licenseNumber}")]
        [ProducesResponseType(typeof(طلب_خدمةDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> IssueReplacement(string licenseNumber, [FromQuery] string type, [FromBody] معلومات_التوصيلDto delivery)
        {
            var nationalId = GetNationalId();

            var englishDelivery = new DeliveryInfoDto
            {
                Method = delivery.طريقة_التوصيل,
                Address = delivery.العنوان != null ? new AddressDto
                {
                    Governorate = delivery.العنوان.المحافظة,
                    City = delivery.العنوان.المدينة,
                    Details = delivery.العنوان.التفاصيل
                } : null
            };

            var result = await _service.IssueReplacementAsync(nationalId, licenseNumber, type, englishDelivery);
            return Ok(MapToArabic(result));
        }

        /// <summary>
        /// Submits a request to renew an existing vehicle license.
        /// </summary>
        /// <param name="apiDto">Renewal details including the license number.</param>
        /// <response code="200">Renewal request submitted successfully.</response>
        /// <response code="400">License not found, already expired beyond renewal period, or has active violations.</response>
        [Authorize(Roles = "CITIZEN")]
        [HttpPost("renew")]
        [ProducesResponseType(typeof(طلب_رخصة_مركبةDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Renew([FromForm] طلب_تجديد_رخصة_مركبةApiDto apiDto)
        {
            var nationalId = GetNationalId();

            var dto = new UploadVehicleDocsDto
            {
                VehicleLicenseNumber = apiDto.رقم_رخصة_المركبة,
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

            return Ok(arabicResult);
        }

        #endregion

        #region Get My Licenses

        /// <summary>
        /// Retrieves all vehicle licenses owned by the currently authenticated citizen.
        /// </summary>
        /// <response code="200">A list of user's vehicle licenses retrieved successfully.</response>
        [Authorize(Roles = "CITIZEN")]
        [HttpGet("my-licenses")]
        [ProducesResponseType(typeof(IEnumerable<رخصة_مركبةDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetMyLicenses()
        {
            var nationalId = GetNationalId();
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

            return Ok(arabicLicenses);
        }

        #endregion
    }
}
