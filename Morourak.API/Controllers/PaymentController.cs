using Microsoft.AspNetCore.Mvc;
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
        if (requestDto == null) return NotFound("Request not found");

        var request = new ServiceRequest { RequestNumber = requestDto.RequestNumber, ServiceType = Enum.Parse<Morourak.Domain.Enums.Request.ServiceType>(requestDto.ServiceType) };

        var token = await _payMobService.GetPaymentTokenAsync(request, amount);
        var iframeUrl = $"https://accept.paymob.com/api/acceptance/iframes/{_settings.IframeId}?payment_token={token}";

        return Ok(new { IframeUrl = iframeUrl });
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook([FromBody] dynamic payload)
    {
        // Note: PayMob sends HMAC in query string for some types of callbacks, 
        // or inside the body for others. Standard 'Transaction' callback usually has it in 'hmac'.
        string? hmac = Request.Query["hmac"];
        if (string.IsNullOrEmpty(hmac))
        {
            // Try to get from body if dynamic allows
            // In a real scenario, use a typed DTO for the webhook payload
        }

        // For simplicity in this implementation, we assume a typed structure exists or we parse it
        // and validate the HMAC. 
        // If validation succeeds:
        // var requestNumber = payload.obj.order.merchant_order_id; // Usually mapped to request number
        // var transactionId = payload.obj.id.ToString();
        // var amount = payload.obj.amount_cents / 100m;
        // var success = payload.obj.success;

        // if (success) {
        //    await _serviceRequestService.MarkAsPaidAsync(requestNumber, transactionId, amount);
        // }

        return Ok();
    }
}
