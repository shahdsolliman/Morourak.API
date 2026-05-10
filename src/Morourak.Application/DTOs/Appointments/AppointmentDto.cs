using Morourak.Domain.Enums.Appointments;
using System.Text.Json.Serialization;

namespace Morourak.Application.DTOs.Appointments
{
    /// <summary>
    /// Data transfer object for appointment details.
    /// </summary>
    public class AppointmentDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("citizenNationalId")]
        public string CitizenNationalId { get; set; } = string.Empty;

        [JsonPropertyName("applicationId")]
        public int ApplicationId { get; set; }

        [JsonPropertyName("type")]
        public AppointmentType Type { get; set; }

        [JsonPropertyName("typeName")]
        public string TypeName { get; set; } = string.Empty;

        [JsonPropertyName("serviceName")]
        public string ServiceName { get; set; } = string.Empty;

        [JsonPropertyName("date")]
        public DateOnly Date { get; set; }

        [JsonPropertyName("dateFormatted")]
        public string DateFormatted { get; set; } = string.Empty;

        [JsonPropertyName("startTime")]
        public TimeOnly StartTime { get; set; }

        [JsonPropertyName("timeFormatted")]
        public string TimeFormatted { get; set; } = string.Empty;

        [JsonPropertyName("endTime")]
        public TimeOnly? EndTime { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("createdAt")]
        public string CreatedAt { get; set; } = string.Empty;

        [JsonPropertyName("completedAt")]
        public string? CompletedAt { get; set; }

        [JsonPropertyName("requestNumberRelated")]
        public string? RequestNumberRelated { get; set; }

        [JsonPropertyName("assignedToUserId")]
        public string AssignedToUserId { get; set; } = string.Empty;

        [JsonPropertyName("governorateId")]
        [JsonIgnore]
        public int GovernorateId { get; set; }

        [JsonPropertyName("trafficUnitId")]
        [JsonIgnore]
        public int TrafficUnitId { get; set; }

        [JsonPropertyName("governorateName")]
        public string GovernorateName { get; set; } = string.Empty;

        [JsonPropertyName("trafficUnitName")]
        public string TrafficUnitName { get; set; } = string.Empty;
    }
}
