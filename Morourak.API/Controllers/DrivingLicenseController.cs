using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Morourak.API.DTOs.DrivingLicenses.Arabic;
using Morourak.API.Errors;
using Morourak.Application.DTOs.Delivery;
using Morourak.Application.DTOs.Delivery.Arabic;
using Morourak.Application.DTOs.DrivingLicenses;
using Morourak.Application.DTOs.DrivingLicenses.Arabic;
using Morourak.Application.DTOs.Licenses;
using Morourak.Application.DTOs.Requests.Arabic;
using Morourak.Application.Interfaces;
using Morourak.Domain.Enums;
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
        /// Uploads initial required documents for a new driving license application.
        /// </summary>
        /// <remarks>
        /// This is the first step in applying for a driving license. The citizen must upload 
        /// their personal photo, educational certificate, ID card, and residence proof.
        /// </remarks>
        /// <param name="apiDto">Multipart form data containing images and license category.</param>
        /// <response code="200">Documents uploaded successfully, application is now pending.</response>
        /// <response code="400">Invalid document format or missing data.</response>
        /// <response code="401">Unauthorized: Citizen role required.</response>
        [HttpPost("upload-documents")]
        [ProducesResponseType(typeof(طلب_رخصة_قيادةDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadDocuments([FromForm] طلب_إصدار_رخصة_قيادة_جديدApiDto apiDto)
        {
            if (apiDto == null)
                throw new AppEx.ValidationException("لم يتم استلام أي بيانات.");

            var nationalId = GetNationalId();

            var dto = new UploadDrivingLicenseDocumentsDto
            {
                PersonalPhoto = await ToByteArrayAsync(apiDto.الصورة_الشخصية),
                EducationalCertificate = await ToByteArrayAsync(apiDto.الشهادة_الدراسية),
                IdCard = await ToByteArrayAsync(apiDto.بطاقة_الرقم_القومي),
                ResidenceProof = await ToByteArrayAsync(apiDto.إثبات_السكن),
                Category = apiDto.الفئة,
                Governorate = apiDto.المحافظة,
                LicensingUnit = apiDto.وحدة_الترخيص
            };

            var result = await _service.UploadInitialDocumentsAsync(nationalId, dto);
            
            // Map result to Arabic DTO
            var arabicResult = new طلب_رخصة_قيادةDto
            {
                Id = result.Id,
                الفئة = result.Category,
                المحافظة = result.Governorate,
                وحدة_الترخيص = result.LicensingUnit,
                الحالة = result.Status,
                رقم_الطلب = result.RequestNumber
            };

            return Ok(arabicResult);
        }

        #endregion

        #region Finalize License

        /// <summary>
        /// Finalizes a driving license application by providing delivery info after all steps are completed.
        /// </summary>
        /// <param name="requestNumber">The unique request number generated during upload.</param>
        /// <param name="delivery">Delivery method and address.</param>
        /// <response code="200">License finalized and issued successfully.</response>
        /// <response code="404">Service request not found or not in finalizable state.</response>
        /// <response code="400">Invalid delivery data or business logic violation.</response>
        [HttpPost("finalize/{requestNumber}")]
        [ProducesResponseType(typeof(طلب_خدمةDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> FinalizeLicense(string requestNumber, [FromBody] معلومات_التوصيلDto delivery)
        {
            try
            {
                var nationalId = GetNationalId();
                
                // Map Arabic DTO back to English for Service
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
                
                // Map result to Arabic DTO
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

                return Ok(arabicResult);
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
        /// Initiates a driving license renewal request.
        /// </summary>
        /// <param name="apiDto">Renewal details (e.g., optional category change).</param>
        /// <response code="200">Renewal request submitted successfully.</response>
        /// <response code="400">Citizen does not have an active license to renew or validation failed.</response>
        [HttpPost("renewal-request")]
        [ProducesResponseType(typeof(طلب_رخصة_قيادةDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SubmitRenewalRequest([FromForm] طلب_تجديد_رخصة_قيادةApiDto apiDto)
        {
            try
            {
                var nationalId = GetNationalId();
                var dto = new SubmitRenewalRequestDto
                {
                    NewCategory = apiDto.الفئة_الجديدة,
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

                return Ok(arabicResult);
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
        /// Finalizes a license renewal by providing delivery info.
        /// </summary>
        /// <param name="requestNumber">The renewal request number.</param>
        /// <param name="delivery">Delivery details.</param>
        /// <response code="200">Renewal finalized successfully.</response>
        /// <response code="404">Renewal request not found.</response>
        [HttpPost("finalize-renewal/{requestNumber}")]
        [ProducesResponseType(typeof(طلب_خدمةDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> FinalizeRenewal(string requestNumber, [FromBody] معلومات_التوصيلDto delivery)
        {
            try
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

                return Ok(arabicResult);
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
        /// Retrieves all driving licenses associated with the currently authenticated citizen.
        /// </summary>
        /// <returns>A list of DrivingLicenseDto.</returns>
        /// <response code="200">Licenses retrieved successfully.</response>
        [HttpGet("my-licenses")]
        [ProducesResponseType(typeof(IEnumerable<رخصة_قيادةDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetMyLicenses()
        {
            try
            {
                var nationalId = GetNationalId();
                var licenses = await _service.GetAllLicensesByCitizenAsync(nationalId);

                if (!licenses.Any())
                    return Ok(new { Message = "لا توجد رخص لهذا المواطن." });

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

                return Ok(arabicLicenses);
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
        /// Requests a replacement for a lost or damaged driving license.
        /// </summary>
        /// <param name="drivingLicenseNumber">The license number to be replaced.</param>
        /// <param name="apiDto">The replacement type (Lost/Damaged) and delivery details.</param>
        /// <response code="200">Replacement license issued successfully.</response>
        /// <response code="404">Original license not found.</response>
        /// <response code="400">Invalid replacement type or existing active request.</response>
        [HttpPost("issue-replacement/{drivingLicenseNumber}")]
        [ProducesResponseType(typeof(طلب_خدمةDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> IssueReplacement(string drivingLicenseNumber, [FromBody] طلب_استخراج_بدل_رخصة_قيادةApiDto apiDto)
        {
            try
            {
                if (apiDto.نوع_البدل != "Lost" && apiDto.نوع_البدل != "Damaged")
                    throw new AppEx.ValidationException("نوع البدل يجب أن يكون 'Lost' (مفقود) أو 'Damaged' (تالف).");

                var nationalId = GetNationalId();
                
                var englishDelivery = new DeliveryInfoDto
                {
                    Method = apiDto.معلومات_التوصيل.طريقة_التوصيل,
                    Address = apiDto.معلومات_التوصيل.العنوان != null ? new AddressDto
                    {
                        Governorate = apiDto.معلومات_التوصيل.العنوان.المحافظة,
                        City = apiDto.معلومات_التوصيل.العنوان.المدينة,
                        Details = apiDto.معلومات_التوصيل.العنوان.التفاصيل
                    } : null
                };

                var result = await _service.IssueReplacementAsync(
                    nationalId,
                    drivingLicenseNumber,
                    apiDto.نوع_البدل,
                    englishDelivery
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

                return Ok(arabicResult);
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
