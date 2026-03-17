using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Morourak.API.Common;
using Morourak.API.DTOs.DrivingLicenses;
using Morourak.Application.DTOs.Delivery;
using Morourak.Application.DTOs.DrivingLicenses;
using Morourak.Application.DTOs.DrivingLicenses.Arabic;
using Morourak.Application.DTOs.Licenses;
using Morourak.Application.DTOs.Requests.Arabic;
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
            
            var arabicResult = new طلب_رخصة_قيادةDto
            {
                Id = result.Id,
                الفئة = result.Category,
                المحافظة = result.Governorate,
                وحدة_الترخيص = result.LicensingUnit,
                الحالة = result.Status,
                رقم_الطلب = result.RequestNumber
            };

            return Ok(ApiResponseArabic.Success(arabicResult, "تم رفع المستندات بنجاح"));
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
            
            var arabicResult = new طلب_خدمةDto
            {
                رقم_الطلب = result.RequestNumber,
                الرقم_القومي = result.CitizenNationalId,
                نوع_الخدمة = result.ServiceType,
                الحالة = result.Status,
                تاريخ_التقديم = result.SubmittedAt,
                تاريخ_آخر_تحديث = result.LastUpdatedAt.GetValueOrDefault(),
                الرقم_المرجعي = result.ReferenceId.ToString(),
                الرسوم = new رسوم_طلب_الخدمةDto
                {
                    الرسوم_الأساسية = result.Fees.BaseFee,
                    رسوم_التوصيل = result.Fees.DeliveryFee,
                    المبلغ_الإجمالي = result.Fees.TotalAmount
                },
                التوصيل = new توصيل_طلب_الخدمةDto
                {
                    طريقة_التوصيل = result.Delivery.Method ?? string.Empty,
                    العنوان = result.Delivery.Address ?? string.Empty
                },
                الدفع = new دفع_طلب_الخدمةDto
                {
                    الحالة = result.Payment.Status,
                    رقم_العملية = result.Payment.TransactionId,
                    المبلغ = result.Payment.Amount ?? 0m,
                    الوقت = result.Payment.Timestamp
                }
            };

            return Ok(ApiResponseArabic.Success(arabicResult, "تم إصدار الرخصة بنجاح"));
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
            
            var arabicResult = new طلب_رخصة_قيادةDto
            {
                Id = result.Id,
                الفئة = result.RequestedCategory,
                المحافظة = result.CurrentCategory,
                الحالة = result.Status switch
                {
                    LicenseStatus.Pending => "قيد الإنتظار",
                    LicenseStatus.Active => "سارية",
                    LicenseStatus.Expired => "منتهية",
                    LicenseStatus.Approved => "تمت الموافقة",
                    LicenseStatus.Completed => "مكتملة",
                    LicenseStatus.Rejected => "مرفوضة",
                    LicenseStatus.Replaced => "تم الاستبدال",
                    LicenseStatus.DocumentsUploaded => "تم رفع المستندات",
                    LicenseStatus.PendingRenewal => "تجديد جاري",
                    LicenseStatus.Withdrawn => "مسحوبة",
                    LicenseStatus.Suspended => "موقوفة",
                    _ => result.Status.ToString()
                },
                رقم_الطلب = result.RequestNumber
            };

            return Ok(ApiResponseArabic.Success(arabicResult, "تم تقديم طلب التجديد بنجاح"));
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
            
            var arabicResult = new طلب_خدمةDto
            {
                رقم_الطلب = result.RequestNumber,
                الرقم_القومي = result.CitizenNationalId,
                نوع_الخدمة = result.ServiceType,
                الحالة = result.Status,
                تاريخ_التقديم = result.SubmittedAt,
                تاريخ_آخر_تحديث = result.LastUpdatedAt.GetValueOrDefault(),
                الرقم_المرجعي = result.ReferenceId.ToString(),
                الرسوم = new رسوم_طلب_الخدمةDto
                {
                    الرسوم_الأساسية = result.Fees.BaseFee,
                    رسوم_التوصيل = result.Fees.DeliveryFee,
                    المبلغ_الإجمالي = result.Fees.TotalAmount
                },
                التوصيل = new توصيل_طلب_الخدمةDto
                {
                    طريقة_التوصيل = result.Delivery.Method ?? string.Empty,
                    العنوان = result.Delivery.Address ?? string.Empty
                },
                الدفع = new دفع_طلب_الخدمةDto
                {
                    الحالة = result.Payment.Status,
                    رقم_العملية = result.Payment.TransactionId,
                    المبلغ = result.Payment.Amount ?? 0m,
                    الوقت = result.Payment.Timestamp
                }
            };

            return Ok(ApiResponseArabic.Success(arabicResult, "تم تجديد الرخصة بنجاح"));
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

            var arabicLicenses = licenses.Select(l => new رخصة_قيادةDto
            {
                رقم_الرخصة = l.LicenseNumber,
                الفئة = l.Category,
                الحالة = l.Status,
                الرقم_القومي = l.CitizenNationalId,
                وحدة_الترخيص = l.LicensingUnit,
                المحافظة = l.Governorate,
                تاريخ_الإصدار = l.IssueDate,
                تاريخ_الانتهاء = l.ExpiryDate
            });

            return Ok(ApiResponseArabic.Success(arabicLicenses));
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

            var arabicResult = new طلب_خدمةDto
            {
                رقم_الطلب = result.RequestNumber,
                الرقم_القومي = result.CitizenNationalId,
                نوع_الخدمة = result.ServiceType,
                الحالة = result.Status,
                تاريخ_التقديم = result.SubmittedAt,
                تاريخ_آخر_تحديث = result.LastUpdatedAt.GetValueOrDefault(),
                الرقم_المرجعي = result.ReferenceId.ToString(),
                الرسوم = new رسوم_طلب_الخدمةDto
                {
                    الرسوم_الأساسية = result.Fees.BaseFee,
                    رسوم_التوصيل = result.Fees.DeliveryFee,
                    المبلغ_الإجمالي = result.Fees.TotalAmount
                },
                التوصيل = new توصيل_طلب_الخدمةDto
                {
                    طريقة_التوصيل = result.Delivery.Method ?? string.Empty,
                    العنوان = result.Delivery.Address ?? string.Empty
                },
                الدفع = new دفع_طلب_الخدمةDto
                {
                    الحالة = result.Payment.Status,
                    رقم_العملية = result.Payment.TransactionId,
                    المبلغ = result.Payment.Amount ?? 0m,
                    الوقت = result.Payment.Timestamp
                }
            };

            return Ok(ApiResponseArabic.Success(arabicResult, "تم استخراج بدل الرخصة بنجاح"));
        }
    }
}
