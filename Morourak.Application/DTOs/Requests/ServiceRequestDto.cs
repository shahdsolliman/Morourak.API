using Morourak.Domain.Enums.Request;
using System;

namespace Morourak.Application.DTOs
{
    /// <summary>
    /// Data transfer object for service request information.
    /// </summary>
    public class ServiceRequestDto
    {
        public string RequestNumber { get; set; } = default!;
        public string CitizenNationalId { get; set; } = default!;
        public string ServiceType { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime SubmittedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public int ReferenceId { get; set; }

        public ServiceRequestFeesDto Fees { get; set; } = new();
        public ServiceRequestDeliveryDto Delivery { get; set; } = new();
        public ServiceRequestPaymentDto Payment { get; set; } = new();
    }

    public class ServiceRequestFeesDto
    {
        public decimal BaseFee { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class ServiceRequestDeliveryDto
    {
        public string? Method { get; set; }
        public string? Address { get; set; }
    }

    public class ServiceRequestPaymentDto
    {
        public string Status { get; set; } = default!;
        public string? TransactionId { get; set; }
        public decimal? Amount { get; set; }
        public DateTime? Timestamp { get; set; }
    }
}