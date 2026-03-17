using System.Text.Json.Serialization;

namespace Morourak.Application.DTOs.DrivingLicenses.Arabic
{
    /// <summary>
    /// Details of a driving license application.
    /// </summary>
    public class طلب_رخصة_قيادةDto
    {
        /// <summary>
        /// Internal unique identifier for the application.
        /// </summary>
        [JsonPropertyName("Id")]
        public int Id { get; set; }

        /// <summary>
        /// Requested license category.
        /// </summary>
        [JsonPropertyName("الفئة")]
        public string الفئة { get; set; } 

        /// <summary>
        /// Selected governorate for the application.
        /// </summary>
        [JsonPropertyName("المحافظة")]
        public string المحافظة { get; set; } = null!; 

        /// <summary>
        /// Selected traffic unit for the application.
        /// </summary>
        [JsonPropertyName("وحدة الترخيص")]
        public string وحدة_الترخيص { get; set; } = null!;

        /// <summary>
        /// Current status of the application (e.g., Pending, Approved).
        /// </summary>
        [JsonPropertyName("الحالة")]
        public string الحالة { get; set; }

        /// <summary>
        /// Public service request number for tracking.
        /// </summary>
        [JsonPropertyName("رقم الطلب")]
        public string رقم_الطلب { get; set; } = string.Empty;
    }
}
