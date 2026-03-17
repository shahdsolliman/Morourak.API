using System.Text.Json.Serialization;
using Morourak.Application.DTOs.Delivery.Arabic;

namespace Morourak.Application.DTOs.DrivingLicenses.Arabic
{
    /// <summary>
    /// Detailed response containing driving license information after finalization or retrieval.
    /// </summary>
    public class نتيجة_رخصة_القيادةDto
    {
        /// <summary>
        /// Internal unique identifier for the license.
        /// </summary>
        [JsonPropertyName("Id")]
        public int Id { get; set; }

        /// <summary>
        /// Publicly visible driving license number.
        /// </summary>
        [JsonPropertyName("رقم رخصة القيادة")]
        public string رقم_رخصة_القيادة { get; set; } = string.Empty;

        /// <summary>
        /// The category of the license.
        /// </summary>
        [JsonPropertyName("الفئة")]
        public string الفئة { get; set; } = string.Empty;

        /// <summary>
        /// Issuing governorate.
        /// </summary>
        [JsonPropertyName("المحافظة")]
        public string المحافظة { get; set; } = string.Empty;

        /// <summary>
        /// Issuing traffic unit.
        /// </summary>
        [JsonPropertyName("وحدة الترخيص")]
        public string وحدة_الترخيص { get; set; } = string.Empty;

        /// <summary>
        /// Date of issuance.
        /// </summary>
        [JsonPropertyName("تاريخ الإصدار")]
        public DateOnly تاريخ_الإصدار { get; set; }

        /// <summary>
        /// Expiration date.
        /// </summary>
        [JsonPropertyName("تاريخ الانتهاء")]
        public DateOnly تاريخ_الانتهاء { get; set; }

        /// <summary>
        /// Current license status.
        /// </summary>
        [JsonPropertyName("الحالة")]
        public string الحالة { get; set; } = string.Empty;

        /// <summary>
        /// Name of the citizen holding the license.
        /// </summary>
        [JsonPropertyName("اسم المواطن")]
        public string اسم_المواطن { get; set; } = string.Empty;

        /// <summary>
        /// Delivery details for the physical license.
        /// </summary>
        [JsonPropertyName("التوصيل")]
        public معلومات_التوصيلDto? التوصيل { get; set; }
    }
}
