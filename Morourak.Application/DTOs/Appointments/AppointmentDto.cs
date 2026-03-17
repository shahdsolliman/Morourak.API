using Morourak.Domain.Enums.Appointments;
using System.Text.Json.Serialization;

namespace Morourak.Application.DTOs.Appointments
{
    /// <summary>
    /// Data transfer object for appointment details.
    /// </summary>
    public class AppointmentDto
    {
        [JsonPropertyName("الرق_القومي_للمواطن")]
        public string CitizenNationalId { get; set; } = string.Empty;

        [JsonPropertyName("معرف_الطلب")]
        public int ApplicationId { get; set; }

        [JsonPropertyName("نوع_الموعد")]
        public AppointmentType Type { get; set; }

        [JsonPropertyName("اسم_النوع")]
        public string TypeName { get; set; } = string.Empty;

        [JsonPropertyName("اسم_الخدمة")]
        public string ServiceName { get; set; } = string.Empty;

        [JsonPropertyName("التاريخ")]
        public DateOnly Date { get; set; }

        [JsonPropertyName("التاريخ_منسق")]
        public string DateFormatted { get; set; } = string.Empty;

        [JsonPropertyName("وقت_البدء")]
        public TimeOnly StartTime { get; set; }

        [JsonPropertyName("الوقت_منسق")]
        public string TimeFormatted { get; set; } = string.Empty;

        [JsonPropertyName("وقت_الانتهاء")]
        public TimeOnly? EndTime { get; set; }

        [JsonPropertyName("الحالة")]
        public AppointmentStatus Status { get; set; }

        [JsonPropertyName("تاريخ_الإنشاء")]
        public string CreatedAt { get; set; } = string.Empty;

        [JsonPropertyName("تاريخ_الاكتمال")]
        public string? CompletedAt { get; set; }

        [JsonPropertyName("رقم_الطلب_المرتبط")]
        public string? RequestNumberRelated { get; set; }

        [JsonPropertyName("معرف_الموظف_المسؤول")]
        public string AssignedToUserId { get; set; } = string.Empty;

        /// <summary>
        /// Internal governorate identifier.
        /// </summary>
        [JsonIgnore]
        public int GovernorateId { get; set; }

        /// <summary>
        /// Internal traffic unit identifier.
        /// </summary>
        [JsonIgnore]
        public int TrafficUnitId { get; set; }

        [JsonPropertyName("اسم_المحافظة")]
        public string GovernorateName { get; set; } = string.Empty;

        [JsonPropertyName("اسم_وحدة_المرور")]
        public string TrafficUnitName { get; set; } = string.Empty;
    }
}
