namespace Morourak.Application.DTOs.Paymob;

public class PaymentReceiptDto
{
    public string TransactionId { get; set; } = string.Empty;
    public string MerchantOrderId { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime PaidAt { get; set; }
    
    public List<ReceiptItemDto> Items { get; set; } = new();
    
    public string CitizenName { get; set; } = string.Empty;
}

public class ReceiptItemDto
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
