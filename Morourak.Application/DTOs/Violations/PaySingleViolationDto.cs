using System.ComponentModel.DataAnnotations;

namespace Morourak.Application.DTOs.Violations
{
    /// <summary>
    /// Request DTO for paying a single violation.
    /// </summary>
    public class PaySingleViolationDto
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }
    }
}
