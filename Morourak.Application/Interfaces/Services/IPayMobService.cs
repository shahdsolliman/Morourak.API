using Morourak.Application.DTOs.Paymob;

namespace Morourak.Application.Interfaces.Services;

public interface IPayMobService
{
    Task<PaymobPaymentResponse> InitiatePaymentAsync(
        decimal amount,
        string merchantOrderId,
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        string country,
        string city,
        string street,
        string building);

    bool ValidateWebhookSignature(string hmacHeader, string requestBody);
}
