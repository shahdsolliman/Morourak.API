namespace Morourak.Application.DTOs
{
    /// <summary>
    /// Lightweight DTO for listing a citizen's service requests (optimized for mobile).
    /// </summary>
    public sealed class ServiceRequestSummaryDto
    {
        public string RequestNumber { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
    }
}

