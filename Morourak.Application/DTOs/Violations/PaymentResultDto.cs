namespace Morourak.Application.DTOs.Violations
{
    /// <summary>
    /// Response DTO for payment operations.
    /// </summary>
    public class PaymentResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public string MessageAr { get; set; } = null!;
        public int ViolationsPaid { get; set; }
        public decimal TotalAmountPaid { get; set; }
        public decimal RemainingBalance { get; set; }
    }
}
