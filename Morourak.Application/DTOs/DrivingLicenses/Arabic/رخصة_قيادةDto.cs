using System.Text.Json.Serialization;

namespace Morourak.Application.DTOs.DrivingLicenses.Arabic
{
    /// <summary>
    /// Represents a driving license issued to a citizen.
    /// </summary>
    public class رخصة_قيادةDto
    {
        /// <summary>Unique license identification number. Keep alphanumeric as requested.</summary>
        [JsonPropertyName("رقم الرخصة")]
        public string رقم_الرخصة { get; set; } = null!;

        /// <summary>
        /// The category of the license (e.g., Private, Professional).
        /// </summary>
        [JsonPropertyName("الفئة")]
        public string الفئة { get; set; } = default!;

        /// <summary>
        /// Current status of the license (e.g., Active, Expired, Suspended).
        /// </summary>
        [JsonPropertyName("الحالة")]
        public string الحالة { get; set; } = default!;

        /// <summary>
        /// The national ID of the license holder.
        /// </summary>
        [JsonPropertyName("الرقم القومي")]
        public string الرقم_القومي { get; set; } = null!;

        /// <summary>
        /// The traffic unit that issued the license.
        /// </summary>
        [JsonPropertyName("وحدة الترخيص")]
        public string وحدة_الترخيص { get; set; } = null!;

        /// <summary>
        /// The governorate where the license was issued.
        /// </summary>
        [JsonPropertyName("المحافظة")]
        public string المحافظة { get; set; } = null!;

        /// <summary>
        /// The date the license was issued.
        /// </summary>
        [JsonPropertyName("تاريخ الإصدار")]
        public DateOnly تاريخ_الإصدار { get; set; }

        /// <summary>
        /// The date the license expires.
        /// </summary>
        [JsonPropertyName("تاريخ الانتهاء")]
        public DateOnly تاريخ_الانتهاء { get; set; }
    }
}
