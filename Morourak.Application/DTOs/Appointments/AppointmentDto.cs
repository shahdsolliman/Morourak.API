using Morourak.Domain.Enums.Appointments;
using System.Text.Json.Serialization;

namespace Morourak.Application.DTOs.Appointments
{
    /// <summary>
    /// Data transfer object for appointment details.
    /// </summary>
    public class AppointmentDto
    {
        /// <summary>
        /// National ID of the citizen who booked the appointment.
        /// </summary>
        public string CitizenNationalId { get; set; } = string.Empty;

        /// <summary>
        /// Associated application ID.
        /// </summary>
        public int ApplicationId { get; set; }

        /// <summary>
        /// Type of appointment as an enum.
        /// </summary>
        public AppointmentType Type { get; set; }

        /// <summary>
        /// Human-readable type name.
        /// </summary>
        public string TypeName { get; set; } = string.Empty;

        /// <summary>
        /// Name of the service related to this appointment.
        /// </summary>
        public string ServiceName { get; set; } = string.Empty;

        /// <summary>
        /// Scheduled date of the appointment.
        /// </summary>
        public DateOnly Date { get; set; }

        /// <summary>
        /// Formatted date string for display.
        /// </summary>
        public string DateFormatted { get; set; } = string.Empty;

        /// <summary>
        /// Scheduled start time.
        /// </summary>
        public TimeOnly StartTime { get; set; }

        /// <summary>
        /// Formatted time string for display.
        /// </summary>
        public string TimeFormatted { get; set; } = string.Empty;

        /// <summary>
        /// Scheduled end time (optional).
        /// </summary>
        public TimeOnly? EndTime { get; set; }

        /// <summary>
        /// Current appointment status (e.g., Scheduled, Completed).
        /// </summary>
        public AppointmentStatus Status { get; set; }

        /// <summary>
        /// Timestamp when the appointment was created.
        /// </summary>
        public string CreatedAt { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp when the appointment was completed (optional).
        /// </summary>
        public string? CompletedAt { get; set; }

        /// <summary>
        /// Tracking number of the related service request.
        /// </summary>
        public string? RequestNumberRelated { get; set; }

        /// <summary>
        /// ID of the staff member assigned to this appointment.
        /// </summary>
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

        /// <summary>
        /// Name of the governorate where the appointment is located.
        /// </summary>
        public string GovernorateName { get; set; } = string.Empty;

        /// <summary>
        /// Name of the traffic unit where the appointment is located.
        /// </summary>
        public string TrafficUnitName { get; set; } = string.Empty;
    }
}
