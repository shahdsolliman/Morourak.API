using Morourak.Domain.Entities;
using Morourak.Domain.Enums.Appointments;

namespace Morourak.Application.DTOs.Appointments
{
    public class AppointmentDto
    {
        public string CitizenNationalId { get; set; }
        public int ApplicationId { get; set; }
        public AppointmentType Type { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
        public AppointmentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? RequestNumber { get; set; }
    }
}
