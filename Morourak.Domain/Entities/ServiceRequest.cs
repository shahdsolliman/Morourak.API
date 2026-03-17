using Morourak.Domain.Common;
using Morourak.Domain.Enums.Request;
using Morourak.Domain.Enums.Common;

namespace Morourak.Domain.Entities;

public class ServiceRequest: BaseEntity<int>
{
    public string RequestNumber { get; set; } = default!; 

    public string CitizenNationalId { get; set; } = default!;

    public ServiceType ServiceType { get; set; }

    public RequestStatus Status { get; set; }

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastUpdatedAt { get; set; }

    public int ReferenceId { get; set; }

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    public string? PaymentTransactionId { get; set; }

    public decimal? PaymentAmount { get; set; }

    public DateTime? PaymentTimestamp { get; set; }

    // New Delivery and Fee Fields
    public DeliveryMethod? DeliveryMethod { get; set; }
    public string? DeliveryAddressDetail { get; set; }
    public decimal BaseFee { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal TotalAmount { get; set; }
}