using System.Text.Json.Serialization;
using Morourak.Domain.Enums.Appointments;

namespace Morourak.Application.DTOs.Appointments.Arabic
{
    /// <summary>
    /// Data transfer object for appointment details in Arabic.
    /// </summary>
    public class بيانات_موعدDto
    {
        [JsonPropertyName("الرقم القومي للمواطن")]
        public string الرقم_القومي_للمواطن { get; set; } = string.Empty;

        [JsonPropertyName("Id")]
        public int Id { get; set; }

        [JsonPropertyName("نوع الموعد")]
        public string نوع_الموعد { get; set; } = string.Empty;

        [JsonPropertyName("اسم الخدمة")]
        public string اسم_الخدمة { get; set; } = string.Empty;

        [JsonPropertyName("التاريخ")]
        public DateOnly التاريخ { get; set; }

        [JsonPropertyName("التاريخ المنسق")]
        public string التاريخ_المنسق { get; set; } = string.Empty;

        [JsonPropertyName("وقت البدء")]
        public TimeOnly وقت_البدء { get; set; }

        [JsonPropertyName("الوقت المنسق")]
        public string الوقت_المنسق { get; set; } = string.Empty;

        [JsonPropertyName("الحالة")]
        public string الحالة { get; set; } = string.Empty;

        [JsonPropertyName("تاريخ الإنشاء")]
        public string تاريخ_الإنشاء { get; set; } = string.Empty;

        [JsonPropertyName("رقم الطلب المرتبط")]
        public string? رقم_الطلب_المرتبط { get; set; }

        [JsonPropertyName("المحافظة")]
        public string المحافظة { get; set; } = string.Empty;

        [JsonPropertyName("وحدة المرور")]
        public string وحدة_المرور { get; set; } = string.Empty;
    }
}
