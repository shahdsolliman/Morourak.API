namespace Morourak.Application.DTOs.Paymob;

public class UnifiedPaymentStatusDto
{
    public string Status { get; set; } = default!;
    public string MerchantOrderId { get; set; } = default!;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EGP";
}
