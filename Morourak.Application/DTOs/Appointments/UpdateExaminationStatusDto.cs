using Morourak.Domain.Enums.Appointments;
using System.ComponentModel.DataAnnotations;

namespace Morourak.Application.DTOs.Appointments
{
    public class UpdateExaminationStatusDto
    {
        [Required]
        public int ExaminationId { get; set; }

        [Required]
        public AppointmentStatus NewStatus { get; set; }
    }
}
