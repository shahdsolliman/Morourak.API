using System.Text.Json.Serialization;
using Morourak.Domain.Enums;
using Morourak.Domain.Enums.Driving;

namespace Morourak.Application.DTOs.Licenses
{
    /// <summary>
    /// Represents a driving license issued to a citizen.
    /// </summary>
    public class DrivingLicenseDto
    {
        [JsonPropertyName("رقم_الرخصة")]
        public string LicenseNumber { get; set; } = null!;

        [JsonPropertyName("الفئة")]
        public string Category { get; set; } = default!;

        [JsonPropertyName("الحالة")]
        public string Status { get; set; } = default!;

        [JsonPropertyName("الرقم_القومي_للمواطن")]
        public string CitizenNationalId { get; set; } = null!;

        [JsonPropertyName("وحدة_الترخيص")]
        public string LicensingUnit { get; set; } = null!;

        [JsonPropertyName("المحافظة")]
        public string Governorate { get; set; } = null!;

        [JsonPropertyName("تاريخ_الإصدار")]
        public DateOnly IssueDate { get; set; }

        [JsonPropertyName("تاريخ_الانتهاء")]
        public DateOnly ExpiryDate { get; set; }
    }
}