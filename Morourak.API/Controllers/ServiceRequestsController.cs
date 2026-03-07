using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppEx = Morourak.Application.Exceptions;
using Morourak.Application.Interfaces.Services;

[Authorize]
[ApiController]
[Route("api/service-requests")]
public class ServiceRequestsController : ControllerBase
{
    private readonly IServiceRequestService _serviceRequestService;

    public ServiceRequestsController(IServiceRequestService serviceRequestService)
    {
        _serviceRequestService = serviceRequestService;
    }

    [HttpGet("my-requests")]
    public async Task<IActionResult> GetMyRequests()
    {
        var requests = await _serviceRequestService.GetCitizenRequestsAsync();
        return Ok(requests);
    }

    [HttpGet("{requestNumber}")]
    public async Task<IActionResult> GetRequestDetails(string requestNumber)
    {
        var request = await _serviceRequestService.GetByRequestNumberAsync(requestNumber);
        if (request == null)
            throw new AppEx.ValidationException("طلب الخدمة غير موجود.", "REQUEST_NOT_FOUND");

        return Ok(request);
    }
}

