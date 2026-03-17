using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;
using Morourak.Application.DTOs.Delivery.Arabic;
using Morourak.Domain.Enums.Common;
using Morourak.Domain.Enums.Driving;

namespace Morourak.API.DTOs.DrivingLicenses.Arabic
{
    /// <summary>
    /// Data required to apply for a first-time driving license (API layer, includes IFormFile for uploads).
    /// </summary>
    public class طلب_إصدار_رخصة_قيادة_جديدApiDto
    {
        /// <summary>
        /// Personal photo of the applicant.
        /// </summary>
        [JsonPropertyName("الصورة الشخصية")]
        public IFormFile الصورة_الشخصية { get; set; } = null!;

        /// <summary>
        /// Scanned educational certificate.
        /// </summary>
        [JsonPropertyName("الشهادة الدراسية")]
        public IFormFile الشهادة_الدراسية { get; set; } = null!;

        /// <summary>
        /// Scanned national ID card.
        /// </summary>
        [JsonPropertyName("بطاقة الرقم القومي")]
        public IFormFile بطاقة_الرقم_القومي { get; set; } = null!;

        /// <summary>
        /// Scanned proof of residence.
        /// </summary>
        [JsonPropertyName("إثبات السكن")]
        public IFormFile إثبات_السكن { get; set; } = null!;

        /// <summary>
        /// Scanned medical certificate.
        /// </summary>
        [JsonPropertyName("الشهادة الطبية")]
        public IFormFile الشهادة_الطبية { get; set; } = null!;

        /// <summary>
        /// Requested driving license category.
        /// </summary>
        [JsonPropertyName("الفئة")]
        public DrivingLicenseCategory الفئة { get; set; }

        /// <summary>
        /// Selected governorate.
        /// </summary>
        [JsonPropertyName("المحافظة")]
        public string المحافظة { get; set; } = null!;

        /// <summary>
        /// Selected traffic unit.
        /// </summary>
        [JsonPropertyName("وحدة الترخيص")]
        public string وحدة_الترخيص { get; set; } = null!;

        /// <summary>
        /// Preferred delivery method.
        /// </summary>
        [JsonPropertyName("طريقة التوصيل")]
        public DeliveryMethod طريقة_التوصيل { get; set; } 

        /// <summary>
        /// Detailed delivery address if applicable.
        /// </summary>
        [JsonPropertyName("عنوان التوصيل")]
        public عنوانDto? عنوان_التوصيل { get; set; } = null;
    }
}
