using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppEx = Morourak.Application.Exceptions;
using Morourak.Application.Interfaces.Services;
using Morourak.Application.DTOs.Requests.Arabic;
using Morourak.API.Errors;

namespace Morourak.API.Controllers
{
    /// <summary>
    /// Controller for managing and tracking citizen service requests.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/service-requests")]
    [Tags("Service Requests")]
    public class ServiceRequestsController : ControllerBase
    {
        private readonly IServiceRequestService _serviceRequestService;

        public ServiceRequestsController(IServiceRequestService serviceRequestService)
        {
            _serviceRequestService = serviceRequestService;
        }

        #region Helpers

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

        /// <summary>
        /// Retrieves all service requests submitted by the currently authenticated citizen.
        /// </summary>
        /// <response code="200">A list of user's service requests retrieved successfully.</response>
        [HttpGet("my-requests")]
        [ProducesResponseType(typeof(IEnumerable<طلب_خدمةDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyRequests()
        {
            var requests = await _serviceRequestService.GetCitizenRequestsAsync();
            var arabicRequests = requests.Select(MapToArabic);
            return Ok(arabicRequests);
        }

        /// <summary>
        /// Retrieves full details of a specific service request by its tracking number.
        /// </summary>
        /// <param name="requestNumber">The unique tracking number of the request.</param>
        /// <response code="200">The service request details retrieved successfully.</response>
        /// <response code="404">Service request not found or does not belong to the user.</response>
        [HttpGet("{requestNumber}")]
        [ProducesResponseType(typeof(طلب_خدمةDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRequestDetails(string requestNumber)
        {
            var request = await _serviceRequestService.GetByRequestNumberAsync(requestNumber);
            if (request == null)
                throw new AppEx.ValidationException("طلب الخدمة غير موجود.", "REQUEST_NOT_FOUND");

            return Ok(MapToArabic(request));
        }
    }
}

