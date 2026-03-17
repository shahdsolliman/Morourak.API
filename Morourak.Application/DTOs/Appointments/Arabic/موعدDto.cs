using System.Text.Json.Serialization;
using Morourak.Domain.Enums.Appointments;

namespace Morourak.Application.DTOs.Appointments.Arabic
{
    /// <summary>
    /// Simplified DTO representing a newly booked appointment in Arabic.
    /// </summary>
    public class موعدDto
    {
        /// <summary>The tracking number of the associated service request.</summary>
        [JsonPropertyName("رقم الخدمة")]
        public string رقم_الخدمة { get; set; } = null!;

        /// <summary>The internal application identifier.</summary>
        [JsonPropertyName("Id")]
        public int Id { get; set; }

        /// <summary>The booked date.</summary>
        [JsonPropertyName("التاريخ")]
        public DateOnly التاريخ { get; set; }

        /// <summary>The booked start time.</summary>
        [JsonPropertyName("وقت البدء")]
        public TimeOnly وقت_البدء { get; set; }

        /// <summary>Initial status of the appointment.</summary>
        [JsonPropertyName("الحالة")]
        public string الحالة { get; set; } = null!;

        /// <summary>National ID of the citizen.</summary>
        [JsonPropertyName("الرقم القومي")]
        public string الرقم_القومي { get; set; } = null!;

        /// <summary>Type of appointment.</summary>
        [JsonPropertyName("نوع الموعد")]
        public string نوع_الموعد { get; set; } = null!;
    }
}
