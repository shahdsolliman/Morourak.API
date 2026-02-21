using Morourak.Domain.Enums.Request;
using System;

namespace Morourak.Application.DTOs
{
    public class ServiceRequestDto
    {
        public string RequestNumber { get; set; } = default!;

        public string CitizenNationalId { get; set; } = default!;

        public string ServiceType { get; set; }

        public string Status { get; set; }

        public DateTime SubmittedAt { get; set; }

        public DateTime? LastUpdatedAt { get; set; }

        public int ReferenceId { get; set; }

        public string PaymentStatus { get; set; } = default!;

        public string? PaymentTransactionId { get; set; }

        public decimal? PaymentAmount { get; set; }

        public DateTime? PaymentTimestamp { get; set; }
    }
}