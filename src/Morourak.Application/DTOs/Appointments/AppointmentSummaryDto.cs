namespace Morourak.Application.DTOs.Appointments;

/// <summary>
/// Lightweight DTO optimized for "My Appointments" (mobile).
/// No Arabic/presentation formatting is applied in Application layer.
/// </summary>
public sealed class AppointmentSummaryDto
{
    public int AppointmentId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty; // ISO-like (yyyy-MM-dd) until API formatter runs
}

