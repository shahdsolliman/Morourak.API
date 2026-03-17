using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Morourak.Application.DTOs.Violations.Arabic
{
    /// <summary>
    /// Request DTO for paying a single traffic violation in Arabic.
    /// </summary>
    public class دفع_مخالفةDto
    {
        /// <summary>The amount to be paid (must be greater than zero).</summary>
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "يجب أن يكون المبلغ أكبر من صفر.")]
        [JsonPropertyName("المبلغ")]
        public decimal المبلغ { get; set; }
    }
}
