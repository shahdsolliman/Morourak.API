using System.Text.Json.Serialization;
using Morourak.Domain.Enums.Vehicles;

namespace Morourak.Application.DTOs.Vehicles.Arabic
{
    /// <summary>
    /// Data transfer object for uploading vehicle-related documents for license applications or renewals.
    /// </summary>
    public class رفع_مستندات_المركبةDto
    {
        /// <summary>
        /// Type of the vehicle.
        /// </summary>
        [JsonPropertyName("نوع المركبة")]
        public VehicleType نوع_المركبة { get; set; }

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
        /// Existing vehicle license number (required for renewals).
        /// </summary>
        [JsonPropertyName("رقم رخصة المركبة")]
        public string? رقم_رخصة_المركبة { get; set; }

        /// <summary>
        /// Ownership proof document (as byte array).
        /// </summary>
        [JsonPropertyName("إثبات الملكية")]
        public byte[]? إثبات_الملكية { get; set; }

        /// <summary>
        /// Vehicle data certificate (as byte array).
        /// </summary>
        [JsonPropertyName("شهادة بيانات المركبة")]
        public byte[]? شهادة_بيانات_المركبة { get; set; }

        /// <summary>
        /// Owner's ID card (as byte array).
        /// </summary>
        [JsonPropertyName("بطاقة الرقم القومي")]
        public byte[]? بطاقة_الرقم_القومي { get; set; }

        /// <summary>
        /// Insurance certificate (as byte array).
        /// </summary>
        [JsonPropertyName("شهادة التأمين")]
        public byte[]? شهادة_التأمين { get; set; }

        /// <summary>
        /// Custom clearance document (as byte array, optional).
        /// </summary>
        [JsonPropertyName("الشهادة الجمركية")]
        public byte[]? الشهادة_الجمركية { get; set; }
    }
}
