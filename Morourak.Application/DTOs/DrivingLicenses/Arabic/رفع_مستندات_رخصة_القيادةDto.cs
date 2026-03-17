using System.Text.Json.Serialization;
using Morourak.Domain.Enums;
using Morourak.Domain.Enums.Driving;

namespace Morourak.Application.DTOs.DrivingLicenses.Arabic
{
    /// <summary>
    /// Data required to upload documents for a driving license application.
    /// </summary>
    public class رفع_مستندات_رخصة_القيادةDto
    {
        /// <summary>
        /// Personal photo of the applicant (as byte array).
        /// </summary>
        [JsonPropertyName("الصورة الشخصية")]
        public byte[] الصورة_الشخصية { get; set; } = null!;

        /// <summary>
        /// Scanned educational certificate (as byte array).
        /// </summary>
        [JsonPropertyName("الشهادة الدراسية")]
        public byte[] الشهادة_الدراسية { get; set; } = null!;

        /// <summary>
        /// Scanned national ID card (as byte array).
        /// </summary>
        [JsonPropertyName("بطاقة الرقم القومي")]
        public byte[] بطاقة_الرقم_القومي { get; set; } = null!;

        /// <summary>
        /// Scanned residence proof (as byte array).
        /// </summary>
        [JsonPropertyName("إثبات السكن")]
        public byte[] إثبات_السكن { get; set; } = null!;

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
        /// Initial status of the application.
        /// </summary>
        [JsonPropertyName("الحالة")]
        public LicenseStatus الحالة { get; set; }
    }
}
