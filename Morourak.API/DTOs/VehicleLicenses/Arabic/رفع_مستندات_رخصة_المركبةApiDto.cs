using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Morourak.Domain.Enums.Vehicles;

namespace Morourak.API.DTOs.VehicleLicenses.Arabic
{
    /// <summary>
    /// Request DTO for uploading vehicle license documents (API layer, includes IFormFile).
    /// </summary>
    public class رفع_مستندات_رخصة_المركبةApiDto
    {
        /// <summary>
        /// Type of vehicle.
        /// </summary>
        [JsonPropertyName("نوع المركبة")]
        public VehicleType نوع_المركبة { get; set; }

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
        /// Ownership proof file.
        /// </summary>
        [JsonPropertyName("إثبات الملكية")]
        public IFormFile إثبات_الملكية { get; set; } = null!;

        /// <summary>
        /// Vehicle data certificate file.
        /// </summary>
        [JsonPropertyName("شهادة بيانات المركبة")]
        public IFormFile شهادة_بيانات_المركبة { get; set; } = null!;

        /// <summary>
        /// Owner's ID card file.
        /// </summary>
        [JsonPropertyName("بطاقة الرقم القومي")]
        public IFormFile بطاقة_الرقم_القومي { get; set; } = null!;

        /// <summary>
        /// Insurance certificate file.
        /// </summary>
        [JsonPropertyName("شهادة التأمين")]
        public IFormFile شهادة_التأمين { get; set; } = null!;

        /// <summary>
        /// Optional custom clearance document file.
        /// </summary>
        [JsonPropertyName("الشهادة الجمركية")]
        public IFormFile? الشهادة_الجمركية { get; set; }
    }
}
