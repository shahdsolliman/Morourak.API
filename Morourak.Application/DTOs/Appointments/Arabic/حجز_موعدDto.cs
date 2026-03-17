using System.Text.Json.Serialization;
using Morourak.Domain.Enums.Appointments;

namespace Morourak.Application.DTOs.Appointments.Arabic
{
    /// <summary>
    /// Request DTO for booking an appointment in Arabic.
    /// </summary>
    public class حجز_موعدDto
    {
        [JsonPropertyName("رقم الطلب")]
        public string رقم_الطلب { get; set; } = null!;

        [JsonPropertyName("نوع الموعد")]
        public AppointmentType نوع_الموعد { get; set; }

        [JsonPropertyName("التاريخ")]
        public DateOnly التاريخ { get; set; }

        [JsonPropertyName("وقت البدء")]
        public TimeOnly وقت_البدء { get; set; }
    }
}
