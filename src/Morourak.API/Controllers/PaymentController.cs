using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Morourak.Application.DTOs.Paymob;
using Morourak.Application.Interfaces.Services;
using System.Text.Json;

namespace Morourak.API.Controllers;

[Route("api/v1/[controller]")]
public class PaymentController : BaseApiController
{
    private readonly IPayMobService _payMobService;
    private readonly IServiceRequestService _serviceRequestService;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentController> _logger;
    private readonly IWebHostEnvironment _env;

    public PaymentController(
        IPayMobService payMobService,
        IServiceRequestService serviceRequestService,
        IPaymentService paymentService,
        ILogger<PaymentController> logger,
        IWebHostEnvironment env)
    {
        _payMobService = payMobService;
        _serviceRequestService = serviceRequestService;
        _paymentService = paymentService;
        _logger = logger;
        _env = env;
    }

    /// <summary>
    /// Creates a Paymob order and returns iframe URL for Flutter WebView.
    /// Requires an authenticated citizen.
    /// </summary>
    [Authorize]
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] PaymentCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid payment creation request received.");
            return BadRequest(ModelState);
        }

        _logger.LogInformation(
            "Creating payment for user. ServiceRequest: {ServiceRequestNumber}, Violations: {ViolationsCount}",
            request.ServiceRequestNumber,
            request.ViolationIds?.Count ?? 0);

        var response = await _paymentService.CreatePaymentAsync(request);
        return Ok(new
        {
            isSuccess = true,
            message = "تم إنشاء عملية الدفع بنجاح",
            errorCode = (string?)null,
            details = response
        });
    }

    /// <summary>
    /// Returns the current status of a payment.
    /// </summary>
    [Authorize]
    [HttpGet("status/{merchantOrderId}")]
    public async Task<IActionResult> GetStatus(string merchantOrderId)
    {
        _logger.LogInformation("Checking status for MerchantOrderId: {MerchantOrderId}", merchantOrderId);

        var status = await _paymentService.GetStatusAsync(merchantOrderId);

        return Ok(new
        {
            isSuccess = true,
            message = (string?)null,
            errorCode = (string?)null,
            details = new
            {
                Status = status.ToString(),
                MerchantOrderId = merchantOrderId
            }
        });
    }

    /// <summary>
    /// Get payment receipt.
    /// </summary>
    [Authorize]
    [HttpGet("receipt/{merchantOrderId}")]
    public async Task<IActionResult> GetReceipt(string merchantOrderId)
    {
        _logger.LogInformation("Retrieving receipt for MerchantOrderId: {MerchantOrderId}", merchantOrderId);

        var receipt = await _paymentService.GetReceiptAsync(merchantOrderId);

        return Ok(new
        {
            isSuccess = true,
            message = (string?)null,
            errorCode = (string?)null,
            details = receipt
        });
    }

    /// <summary>
    /// User redirect callback after payment completion.
    /// Used only for testing before Flutter integration.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("callback")]
    public IActionResult Callback(
        [FromQuery] bool success,
        [FromQuery] string? merchant_order_id)
    {
        _logger.LogInformation(
            "User returned from Paymob. MerchantOrderId: {MerchantOrderId}, Success: {Success}",
            merchant_order_id,
            success);

        if (success)
            return Content(GenerateSuccessHtml(), "text/html");

        return Content(GenerateFailedHtml(), "text/html");
    }

    /// <summary>
    /// Handles Paymob transaction webhooks with HMAC validation.
    /// </summary>
    [HttpPost("paymob-callback")]
    public async Task<IActionResult> PaymobCallback()
    {
        try
        {
            if (!Request.Headers.TryGetValue("hmac", out var hmacHeader))
            {
                _logger.LogWarning("Paymob webhook missing HMAC header");
                return BadRequest("Missing HMAC");
            }

            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            bool skipValidation = _env.IsDevelopment() && hmacHeader == "test";

            if (!skipValidation && !_payMobService.ValidateWebhookSignature(hmacHeader!, body))
            {
                _logger.LogWarning(
                    "INVALID_HMAC_REJECTED: Webhook rejected due to HMAC mismatch. Possible spoofing attempt. IP={IP}",
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
                return Unauthorized("Invalid Signature");
            }

            using var payload = JsonDocument.Parse(body);
            var obj = payload.RootElement.GetProperty("obj");

            _logger.LogInformation(
                "Paymob webhook obj keys: {Keys}",
                string.Join(", ", obj.EnumerateObject().Select(p => p.Name)));

            bool success = false;
            if (obj.TryGetProperty("success", out var successProp))
            {
                success = successProp.ValueKind switch
                {
                    JsonValueKind.True   => true,
                    JsonValueKind.False  => false,
                    JsonValueKind.String => bool.TryParse(successProp.GetString(), out var b) && b,
                    _                    => false
                };
            }

            if (!obj.TryGetProperty("id", out var transactionIdProp))
            {
                _logger.LogError("Paymob webhook missing 'id' field. Body keys: {Keys}",
                    string.Join(", ", obj.EnumerateObject().Select(p => p.Name)));
                return BadRequest("Missing transaction id in webhook payload.");
            }
            var transactionId = transactionIdProp.ToString();

            if (!obj.TryGetProperty("order", out var orderProp))
            {
                _logger.LogError("Paymob webhook missing 'order' field.");
                return BadRequest("Missing order in webhook payload.");
            }

            if (!orderProp.TryGetProperty("id", out var paymobOrderIdProp))
            {
                _logger.LogError("Paymob webhook 'order' missing 'id' field.");
                return BadRequest("Missing order.id in webhook payload.");
            }
            var paymobOrderId = paymobOrderIdProp.ToString();

            string? merchantOrderId = null;
            if (orderProp.TryGetProperty("merchant_order_id", out var merchantOrderIdProp))
                merchantOrderId = merchantOrderIdProp.GetString();

            bool errorOccured = false;
            if (obj.TryGetProperty("error_occured", out var errProp))
                errorOccured = errProp.GetBoolean();

            string? failureMessage = null;
            if (obj.TryGetProperty("data", out var dataProp) &&
                dataProp.TryGetProperty("message", out var msgProp))
            {
                failureMessage = msgProp.GetString();
            }

            _logger.LogInformation(
                "Valid webhook received for PaymobOrderId: {PaymobOrderId}, MerchantOrderId: {MerchantOrderId}. Success: {Success}, ErrorOccured: {ErrorOccured}, Message: {Message}",
                paymobOrderId,
                merchantOrderId,
                success,
                errorOccured,
                failureMessage ?? "N/A");

            var isSuccess = success; 

            var finalized = await _paymentService.FinalizePaymentAsync(paymobOrderId, transactionId, isSuccess, merchantOrderId);

            if (!finalized)
            {
                _logger.LogWarning(
                    "Payment finalization returned false for MerchantOrderId: {MerchantOrderId}, PaymobOrderId: {PaymobOrderId}",
                    merchantOrderId ?? "N/A",
                    paymobOrderId);
            }

            return Ok();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON payload in Paymob webhook.");
            return BadRequest("Invalid JSON");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error during Paymob webhook processing.");
            return StatusCode(500, "Internal Server Error during callback processing");
        }
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook() => await PaymobCallback();

    private string GenerateSuccessHtml()
    {
        return """
        <html>
        <head>
            <title>Payment Success</title>
            <style>
                body{font-family:Arial;text-align:center;margin-top:80px;background:#f4f6f8;}
                .card{background:white;padding:40px;border-radius:10px;width:400px;margin:auto;box-shadow:0 2px 10px rgba(0,0,0,0.1);}
                .success{color:green;font-size:28px;}
            </style>
        </head>
        <body>
            <div class="card">
                <h1 class="success">Payment Successful</h1>
                <p>Your payment was processed successfully.</p>
                <p>You may now return to the application.</p>
            </div>
        </body>
        </html>
        """;
    }

    private string GenerateFailedHtml()
    {
        return """
        <html>
        <head>
            <title>Payment Failed</title>
            <style>
                body{font-family:Arial;text-align:center;margin-top:80px;background:#f4f6f8;}
                .card{background:white;padding:40px;border-radius:10px;width:400px;margin:auto;box-shadow:0 2px 10px rgba(0,0,0,0.1);}
                .fail{color:red;font-size:28px;}
            </style>
        </head>
        <body>
            <div class="card">
                <h1 class="fail">Payment Failed</h1>
                <p>Your payment could not be completed.</p>
                <p>Please try again.</p>
            </div>
        </body>
        </html>
        """;
    }
}