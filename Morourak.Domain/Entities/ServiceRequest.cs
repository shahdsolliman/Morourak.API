using Morourak.Domain.Common;
using Morourak.Domain.Enums.Request;

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
}