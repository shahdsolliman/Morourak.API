using Morourak.Domain.Enums.Violations;
using System.Text.Json.Serialization;

namespace Morourak.Application.DTOs.Violations
{
    /// <summary>
    /// Summary DTO for violation list views.
    /// </summary>
    /// <summary>
    /// Data transfer object representing a traffic violation.
    /// </summary>
    public class ViolationDto
    {
        [JsonPropertyName("المعرف")]
        public int ViolationId { get; set; }

        [JsonPropertyName("رقم_المخالفة")]
        public string ViolationNumber { get; set; } = null!;

        [JsonPropertyName("نوع_المخالفة")]
        public string ViolationType { get; set; } = null!;

        [JsonPropertyName("السند_القانوني")]
        public string LegalReference { get; set; } = null!;

        [JsonPropertyName("الوصف")]
        public string Description { get; set; } = null!;

        [JsonPropertyName("الموقع")]
        public string Location { get; set; } = null!;

        [JsonPropertyName("تاريخ_ووقت_المخالفة")]
        public string ViolationDateTime { get; set; } = null!;

        [JsonPropertyName("مبلغ_الغرامة")]
        public decimal FineAmount { get; set; }

        [JsonPropertyName("المبلغ_المدفوع")]
        public decimal PaidAmount { get; set; }

        [JsonPropertyName("المبلغ_المتبقي")]
        public decimal RemainingAmount => FineAmount - PaidAmount;

        [JsonPropertyName("الحالة")]
        public ViolationStatus Status { get; set; }

        [JsonPropertyName("الحالة_بالعربي")]
        public string StatusAr { get; set; } = null!;

        [JsonPropertyName("قابل_للدفع")]
        public bool IsPayable { get; set; }
    }
}
