using Morourak.Domain.Enums.Appointments;

namespace Morourak.Application.DTOs.Appointments
{
    public class BookAppointmentRequestDto
    {
        public string RequestNumber { get; set; }
        public AppointmentType Type { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
    }
}
