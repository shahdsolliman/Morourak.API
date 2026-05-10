using Morourak.Domain.Enums.Appointments;

namespace Morourak.Application.DTOs.Appointments
{
    public class UpdateStatusRequestDto
    {
        public AppointmentStatus Status { get; set; }
        public string? Notes { get; set; }
    }
}
