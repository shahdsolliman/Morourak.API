using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using AppEx = Morourak.Application.Exceptions;
using Morourak.Application.Interfaces.Services;
using Morourak.Application.CQRS.Requests.Queries.GetMyRequests;
using Morourak.Application.DTOs.Common;
using Morourak.Application.DTOs;

namespace Morourak.API.Controllers
{
    /// <summary>
    /// Controller for managing and tracking citizen service requests.
    /// </summary>
    [Authorize]
    [Route("api/v1/[controller]")]
    [Tags("Service Requests")]
    public class ServiceRequestsController : BaseApiController
    {
        private readonly IMediator _mediator;
        private readonly IServiceRequestService _serviceRequestService;

        public ServiceRequestsController(
            IMediator mediator,
            IServiceRequestService serviceRequestService)
        {
            _mediator = mediator;
            _serviceRequestService = serviceRequestService;
        }


        /// <summary>
        /// Retrieves all service requests submitted by the currently authenticated citizen.
        /// </summary>
        [HttpGet("my-requests")]
        public async Task<IActionResult> GetMyRequests([FromQuery] PaginationParams pagination)
        {
            var result = await _mediator.Send(
                new GetMyRequestsQuery(NationalId, pagination));

            return Ok(new
            {
                isSuccess = true,
                message = "Success",
                errorCode = (string?)null,
                data = result,
                details = result
            });
        }

        /// <summary>
        /// Retrieves full details of a specific service request by its tracking number.
        /// </summary>
        [HttpGet("{requestNumber}")]
        public async Task<IActionResult> GetRequestDetails(string requestNumber)
        {
            var request = await _serviceRequestService.GetByRequestNumberAsync(requestNumber);
            if (request == null)
                throw new AppEx.ValidationException("طلب الخدمة غير موجود.", "REQUEST_NOT_FOUND");

            return Ok(new
            {
                isSuccess = true,
                message = (string?)null,
                errorCode = (string?)null,
                details = request
            });
        }
    }
}
