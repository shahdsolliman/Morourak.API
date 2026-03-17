using System.ComponentModel.DataAnnotations;

namespace Morourak.Application.DTOs.Violations
{
    /// <summary>
    /// Request DTO for paying a single violation.
    /// </summary>
    /// <summary>
    /// Request DTO for paying a single traffic violation.
    /// </summary>
    public class PaySingleViolationDto
    {
        /// <summary>The amount to be paid (must be greater than zero).</summary>
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }
    }
}
