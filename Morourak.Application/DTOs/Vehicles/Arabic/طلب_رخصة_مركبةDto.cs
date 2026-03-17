using System.Text.Json.Serialization;

namespace Morourak.Application.DTOs.Vehicles.Arabic
{
    /// <summary>
    /// Details of a vehicle license application.
    /// </summary>
    public class طلب_رخصة_مركبةDto
    {
        /// <summary>
        /// Internal unique identifier.
        /// </summary>
        [JsonPropertyName("Id")]
        public int Id { get; set; }

        /// <summary>
        /// Type of vehicle for the application.
        /// </summary>
        [JsonPropertyName("نوع المركبة")]
        public string نوع_المركبة { get; set; } = null!;

        /// <summary>
        /// Vehicle brand.
        /// </summary>
        [JsonPropertyName("الماركة")]
        public string الماركة { get; set; } = null!;

        /// <summary>
        /// Vehicle model.
        /// </summary>
        [JsonPropertyName("الموديل")]
        public string الموديل { get; set; } = null!;

        /// <summary>
        /// Current application status.
        /// </summary>
        [JsonPropertyName("الحالة")]
        public string الحالة { get; set; } = null!;

        /// <summary>
        /// Service request number for tracking.
        /// </summary>
        [JsonPropertyName("رقم الطلب")]
        public string رقم_الطلب { get; set; } = null!;
    }
}
