using Morourak.Domain.Common;
using Morourak.Domain.Enums.Request;

namespace Morourak.Domain.Entities;

public class Payment : BaseEntity<int>
{
    public string? TransactionId { get; set; }
    public string MerchantOrderId { get; set; } = null!;
    public string? PaymobOrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EGP";
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string CitizenNationalId { get; set; } = null!;
    
    public string? ServiceRequestNumber { get; set; }
    public ServiceRequest? ServiceRequest { get; set; }

    public ICollection<PaymentViolation> PaymentViolations { get; set; } = new List<PaymentViolation>();
    public ICollection<PaymentItem> PaymentItems { get; set; } = new List<PaymentItem>();

    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
