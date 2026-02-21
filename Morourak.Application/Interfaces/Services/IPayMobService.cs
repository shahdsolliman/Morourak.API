using Morourak.Domain.Entities;

namespace Morourak.Application.Interfaces.Services;

public interface IPayMobService
{
    /// <summary>
    /// Authenticates with PayMob, creates an order, and returns a payment key token (Step 3 of PayMob flow).
    /// </summary>
    Task<string> GetPaymentTokenAsync(ServiceRequest request, decimal amount);

    /// <summary>
    /// Validates the HMAC signature of a PayMob webhook callback.
    /// </summary>
    Task<bool> ValidateWebhookSignatureAsync(IDictionary<string, string> payload, string hmac);
}
