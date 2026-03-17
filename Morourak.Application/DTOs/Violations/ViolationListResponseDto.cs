using System.Text.Json.Serialization;

namespace Morourak.Application.DTOs.Violations
{
    /// <summary>
    /// Response wrapper for violation list queries.
    /// Includes summary statistics for UI display.
    /// </summary>
    public class ViolationListResponseDto
    {
        [JsonPropertyName("المخالفات")]
        public List<ViolationDto> Violations { get; set; } = new();

        [JsonPropertyName("إجمالي_العدد")]
        public int TotalCount { get; set; }

        [JsonPropertyName("عدد_غير_المدفوع")]
        public int UnpaidCount { get; set; }

        [JsonPropertyName("إجمالي_المبلغ_المطلوب")]
        public decimal TotalPayableAmount { get; set; }

        [JsonPropertyName("الرسالة")]
        public string Message { get; set; } = null!;

        [JsonPropertyName("الرسالة_بالعربي")]
        public string MessageAr { get; set; } = null!;
    }
}
