using Morourak.Domain.Enums.Request;

namespace Morourak.Application.DTOs.Paymob;

public class PaymentStatusResult
{
    public PaymentStatus Status { get; set; }
    public string? TransactionId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EGP";
}
