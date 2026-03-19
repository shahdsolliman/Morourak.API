namespace Morourak.Application.DTOs.Appointments;

/// <summary>
/// Detailed DTO for a single appointment (used by legacy/detailed views).
/// </summary>
public sealed class AppointmentDetailsDto
{
    public int AppointmentId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;

    public string StartTime { get; set; } = string.Empty;
    public string? EndTime { get; set; }

    public string GovernorateName { get; set; } = string.Empty;
    public string TrafficUnitName { get; set; } = string.Empty;

    public string RequestNumberRelated { get; set; } = string.Empty;
    public string AssignedToUserId { get; set; } = string.Empty;
}

