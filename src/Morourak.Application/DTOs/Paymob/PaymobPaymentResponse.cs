namespace Morourak.Application.DTOs.Paymob;

public class PaymobPaymentResponse
{
    public string PaymentToken { get; set; } = string.Empty;
    public int PaymentId { get; set; }
    public string PaymobOrderId { get; set; } = string.Empty;
    public string MerchantOrderId { get; set; } = string.Empty;
    public string PaymentUrl { get; set; } = string.Empty;


}
