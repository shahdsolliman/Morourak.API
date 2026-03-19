using Morourak.Domain.Enums.Appointments;
using System.Text.Json.Serialization;

namespace Morourak.Application.DTOs.Appointments;

/// <summary>
/// Simplified DTO representing a newly booked appointment.
/// </summary>
public class BookedAppointmentDto
{
    /// <summary>The tracking number of the associated service request.</summary>
    public string ServiceNumber { get; set; } = null!;

    /// <summary>The internal application identifier.</summary>
    public int ApplicationId { get; set; }

    /// <summary>The booked date.</summary>
    public DateOnly Date { get; set; }

    /// <summary>The booked start time.</summary>
    public TimeOnly StartTime { get; set; }

    /// <summary>Initial status of the appointment.</summary>
    public AppointmentStatus Status { get; set; }

    /// <summary>National ID of the citizen.</summary>
    public string NationalId { get; set; } = null!;

    /// <summary>Type of appointment.</summary>
    public AppointmentType Type { get; set; }
}