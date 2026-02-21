namespace Morourak.Application.DTOs.Violations
{
    /// <summary>
    /// Response wrapper for violation list queries.
    /// Includes summary statistics for UI display.
    /// </summary>
    public class ViolationListResponseDto
    {
        public List<ViolationDto> Violations { get; set; } = new();
        public int TotalCount { get; set; }
        public int UnpaidCount { get; set; }
        public decimal TotalPayableAmount { get; set; }
        public string Message { get; set; } = null!;
        public string MessageAr { get; set; } = null!;
    }
}
