using System.Text.Json.Serialization;

namespace Morourak.Application.DTOs.Appointments
{
    /// <summary>
    /// كائن نقل بيانات لمواعيد الخدمات بأسماء خصائص عربية واضحة.
    /// </summary>
    public class AppointmentArabicResponseDto
    {
        /// <summary>
        /// رقم الطلب المرتبط بالموعد.
        /// </summary>
        [JsonPropertyName("رقم_الطلب")]
        public string رقم_الطلب { get; set; } = string.Empty;

        /// <summary>
        /// الرقم القومي للمواطن صاحب الموعد.
        /// </summary>
        [JsonPropertyName("الرقم_القومي_للمواطن")]
        public string الرقم_القومي_للمواطن { get; set; } = string.Empty;

        /// <summary>
        /// نوع الموعد (طبي، فني، قيادة).
        /// </summary>
        [JsonPropertyName("نوع_الموعد")]
        public string نوع_الموعد { get; set; } = string.Empty;

        /// <summary>
        /// اسم وحدة المرور التابع لها الموعد.
        /// </summary>
        [JsonPropertyName("اسم_وحدة_المرور")]
        public string اسم_وحدة_المرور { get; set; } = string.Empty;

        /// <summary>
        /// المحافظة التابع لها الموعد.
        /// </summary>
        [JsonPropertyName("المحافظة")]
        public string المحافظة { get; set; } = string.Empty;

        /// <summary>
        /// تاريخ انتهاء حالة الرخصة الحالية إن وجد.
        /// </summary>
        [JsonPropertyName("تاريخ_انتهاء_حالة_الرخصة")]
        public string? تاريخ_انتهاء_حالة_الرخصة { get; set; }

        /// <summary>
        /// تاريخ ووقت الحجز بتنسيق عربي كامل.
        /// يتم تنسيقه مثل: "14 مارس 2026 10:00 صباحاً"
        /// </summary>
        [JsonPropertyName("تاريخ_ووقت_الحجز")]
        public string تاريخ_ووقت_الحجز { get; set; } = string.Empty;

        /// <summary>
        /// حالة الموعد الحالية (مؤكد، ملغي، مكتمل).
        /// </summary>
        [JsonPropertyName("حالة_الموعد")]
        public string حالة_الموعد { get; set; } = string.Empty;
    }
}
