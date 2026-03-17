using System.Text.Json.Serialization;
using Morourak.Domain.Enums.Driving;

namespace Morourak.API.DTOs.DrivingLicenses.Arabic
{
    /// <summary>
    /// Request DTO for submitting a driving license renewal request.
    /// </summary>
    public class طلب_تجديد_رخصة_قيادةApiDto
    {
        /// <summary>
        /// The new license category requested (if upgrading).
        /// </summary>
        [JsonPropertyName("الفئة الجديدة")]
        public DrivingLicenseCategory? الفئة_الجديدة { get; set; }
    }
}
