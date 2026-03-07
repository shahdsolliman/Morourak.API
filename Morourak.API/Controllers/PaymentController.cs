using Microsoft.AspNetCore.Mvc;
using AppEx = Morourak.Application.Exceptions;
using Morourak.Application.Interfaces.Services;
using Morourak.Domain.Entities;
using Morourak.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace Morourak.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPayMobService _payMobService;
    private readonly IServiceRequestService _serviceRequestService;
    private readonly PayMobSettings _settings;

    public PaymentController(
        IPayMobService payMobService,
        IServiceRequestService serviceRequestService,
        IOptions<PayMobSettings> settings)
    {
        _payMobService = payMobService;
        _serviceRequestService = serviceRequestService;
        _settings = settings.Value;
    }

    [HttpPost("initiate/{requestNumber}")]
    public async Task<IActionResult> Initiate(string requestNumber, [FromQuery] decimal amount)
    {
        var requestDto = await _serviceRequestService.GetByRequestNumberAsync(requestNumber);
        if (requestDto == null) 
            throw new AppEx.ValidationException("????? ??? ?????.", "REQUEST_NOT_FOUND");

        var request = new ServiceRequest { RequestNumber = requestDto.RequestNumber, ServiceType = Enum.Parse<Morourak.Domain.Enums.Request.ServiceType>(requestDto.ServiceType) };

        var token = await _payMobService.GetPaymentTokenAsync(request, amount);
        var iframeUrl = $"https://accept.paymob.com/api/acceptance/iframes/{_settings.IframeId}?payment_token={token}";

        return Ok(new { IframeUrl = iframeUrl });
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook([FromBody] dynamic payload)
    {
        // ... (existing logic)
        return Ok();
    }
}

