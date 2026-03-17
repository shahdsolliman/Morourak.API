using System.Text.Json.Serialization;
using Morourak.Application.DTOs.Delivery.Arabic;

namespace Morourak.Application.DTOs.Vehicles.Arabic
{
    /// <summary>
    /// Response containing full details of a vehicle license.
    /// </summary>
    public class نتيجة_رخصة_مركبةDto
    {
        /// <summary>
        /// Internal identifier.
        /// </summary>
        [JsonPropertyName("Id")]
        public int Id { get; set; }

        /// <summary>
        /// Official vehicle license number.
        /// </summary>
        [JsonPropertyName("رقم رخصة المركبة")]
        public string رقم_رخصة_المركبة { get; set; } = null!;

        /// <summary>
        /// Vehicle license plate number.
        /// </summary>
        [JsonPropertyName("رقم اللوحة")]
        public string رقم_اللوحة { get; set; } = null!;

        /// <summary>
        /// Vehicle type.
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
        /// License status.
        /// </summary>
        [JsonPropertyName("الحالة")]
        public string الحالة { get; set; } = null!;

        /// <summary>
        /// Date of issuance.
        /// </summary>
        [JsonPropertyName("تاريخ الإصدار")]
        public DateOnly تاريخ_الإصدار { get; set; }

        /// <summary>
        /// Date of expiration.
        /// </summary>
        [JsonPropertyName("تاريخ الانتهاء")]
        public DateOnly تاريخ_الانتهاء { get; set; }

        /// <summary>
        /// Owner's national ID.
        /// </summary>
        [JsonPropertyName("الرقم القومي")]
        public string الرقم_القومي { get; set; } = null!;

        /// <summary>
        /// Owner's full name.
        /// </summary>
        [JsonPropertyName("اسم المواطن")]
        public string اسم_المواطن { get; set; } = null!;

        /// <summary>
        /// Delivery details for the physical license.
        /// </summary>
        [JsonPropertyName("التوصيل")]
        public معلومات_التوصيلDto للتوصيل { get; set; } = null!;
    }
}
