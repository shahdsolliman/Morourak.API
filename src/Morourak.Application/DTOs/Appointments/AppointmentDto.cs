using Morourak.Domain.Enums.Appointments;
using System.Text.Json.Serialization;

namespace Morourak.Application.DTOs.Appointments
{
    /// <summary>
    /// Data transfer object for appointment details.
    /// </summary>
    public class AppointmentDto
    {
        public string CitizenNationalId { get; set; } = string.Empty;
        public int ApplicationId { get; set; }
        public AppointmentType Type { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public DateOnly Date { get; set; }
        public string DateFormatted { get; set; } = string.Empty;
        public TimeOnly StartTime { get; set; }
        public string TimeFormatted { get; set; } = string.Empty;
        public TimeOnly? EndTime { get; set; }
        public AppointmentStatus Status { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
        public string? CompletedAt { get; set; }
        public string? RequestNumberRelated { get; set; }
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

        public string GovernorateName { get; set; } = string.Empty;
        public string TrafficUnitName { get; set; } = string.Empty;
    }
}
