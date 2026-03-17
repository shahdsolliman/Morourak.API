using System.Text.Json.Serialization;

namespace Morourak.Application.DTOs.Vehicles.Arabic
{
    /// <summary>
    /// Represents a vehicle license issued to a citizen.
    /// </summary>
    public class رخصة_مركبةDto
    {
        /// <summary>
        /// Internal unique identifier.
        /// </summary>
        [JsonPropertyName("Id")]
        public int Id { get; set; }

        /// <summary>
        /// Unique vehicle license number.
        /// </summary>
        [JsonPropertyName("رقم رخصة المركبة")]
        public string رقم_رخصة_المركبة { get; set; } = null!;

        /// <summary>
        /// The vehicle's license plate number.
        /// </summary>
        [JsonPropertyName("رقم اللوحة")]
        public string رقم_اللوحة { get; set; } = null!;

        /// <summary>
        /// Type of vehicle (e.g., Car, Motorcycle).
        /// </summary>
        [JsonPropertyName("نوع المركبة")]
        public string نوع_المركبة { get; set; } = null!;

        /// <summary>
        /// Vehicle manufacturer brand.
        /// </summary>
        [JsonPropertyName("الماركة")]
        public string الماركة { get; set; } = null!;

        /// <summary>
        /// Vehicle model name.
        /// </summary>
        [JsonPropertyName("الموديل")]
        public string الموديل { get; set; } = null!;

        /// <summary>
        /// Current status of the license.
        /// </summary>
        [JsonPropertyName("الحالة")]
        public string الحالة { get; set; } = null!;

        /// <summary>
        /// Date the license was issued.
        /// </summary>
        [JsonPropertyName("تاريخ الإصدار")]
        public DateTime تاريخ_الإصدار { get; set; }

        /// <summary>
        /// Date the license expires.
        /// </summary>
        [JsonPropertyName("تاريخ الانتهاء")]
        public DateTime تاريخ_الانتهاء { get; set; }

        /// <summary>
        /// National ID of the vehicle owner.
        /// </summary>
        [JsonPropertyName("الرقم القومي")]
        public string الرقم_القومي { get; set; } = null!;
    }
}
