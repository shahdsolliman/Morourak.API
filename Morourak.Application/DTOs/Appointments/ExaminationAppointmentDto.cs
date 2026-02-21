using Morourak.Domain.Enums.Appointments;

namespace Morourak.Application.DTOs.Appointments;

public class ExaminationAppointmentDto
{
    public int Id { get; set; }
    public string CitizenNationalId { get; set; } = null!;
    public AppointmentType Type { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public AppointmentStatus Status { get; set; }
}
