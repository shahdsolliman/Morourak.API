using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Morourak.API.Common;
using AppEx = Morourak.Application.Exceptions;
using Morourak.Application.Interfaces.Services;

namespace Morourak.API.Controllers
{
    /// <summary>
    /// Controller for managing and tracking citizen service requests.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/v1/service-requests")]
    [Tags("Service Requests")]
    public class ServiceRequestsController : BaseApiController
    {
        private readonly IServiceRequestService _serviceRequestService;

        public ServiceRequestsController(IServiceRequestService serviceRequestService)
        {
            _serviceRequestService = serviceRequestService;
        }


        /// <summary>
        /// Retrieves all service requests submitted by the currently authenticated citizen.
        /// </summary>
        [HttpGet("my-requests")]
        [ProducesResponseType(typeof(ApiResponseArabic), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyRequests()
        {
            var requests = await _serviceRequestService.GetCitizenRequestsAsync();
            return Ok(ApiResponseArabic.Success(requests));
        }

        /// <summary>
        /// Retrieves full details of a specific service request by its tracking number.
        /// </summary>
        [HttpGet("{requestNumber}")]
        [ProducesResponseType(typeof(ApiResponseArabic), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseArabic), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRequestDetails(string requestNumber)
        {
            var request = await _serviceRequestService.GetByRequestNumberAsync(requestNumber);
            if (request == null)
                throw new AppEx.ValidationException("طلب الخدمة غير موجود.", "REQUEST_NOT_FOUND");

            return Ok(ApiResponseArabic.Success(request));
        }
    }
}
