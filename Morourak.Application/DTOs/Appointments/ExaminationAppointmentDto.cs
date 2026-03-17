using Morourak.Domain.Enums.Appointments;

namespace Morourak.Application.DTOs.Appointments;

/// <summary>
/// Details of an examination-related appointment.
/// </summary>
public class ExaminationAppointmentDto
{
    /// <summary>Internal unique identifier.</summary>
    public int Id { get; set; }

    /// <summary>National ID of the citizen.</summary>
    public string CitizenNationalId { get; set; } = null!;

    /// <summary>Type of appointment.</summary>
    public AppointmentType Type { get; set; }

    /// <summary>Date of examination.</summary>
    public DateOnly Date { get; set; }

    /// <summary>Start time of the appointment.</summary>
    public TimeOnly StartTime { get; set; }

    /// <summary>End time of the appointment.</summary>
    public TimeOnly EndTime { get; set; }

    /// <summary>Current appointment status.</summary>
    public AppointmentStatus Status { get; set; }
}
