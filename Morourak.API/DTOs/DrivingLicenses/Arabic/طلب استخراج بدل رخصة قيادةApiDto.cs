using System.Text.Json.Serialization;
using Morourak.Application.DTOs.Delivery.Arabic;

namespace Morourak.API.DTOs.DrivingLicenses.Arabic
{
    /// <summary>
    /// Data required to issue a replacement for a driving license.
    /// </summary>
    public class طلب_استخراج_بدل_رخصة_قيادةApiDto
    {
        /// <summary>
        /// The reason for replacement (e.g., "Lost" or "Damaged").
        /// </summary>
        /// <example>Lost</example>
        [JsonPropertyName("نوع البدل")]
        public string نوع_البدل { get; set; } = null!;

        /// <summary>
        /// Delivery information and address for the new license.
        /// </summary>
        [JsonPropertyName("معلومات التوصيل")]
        public معلومات_التوصيلDto معلومات_التوصيل { get; set; } = null!;
    }
}
