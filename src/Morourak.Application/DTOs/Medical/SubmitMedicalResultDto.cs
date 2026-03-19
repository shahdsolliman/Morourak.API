using Morourak.Domain.Enums.Appointments;
using System.ComponentModel.DataAnnotations;

namespace Morourak.Application.DTOs.Medical
{
    public class SubmitMedicalResultDto
    {
        [Required]
        public int ExaminationId { get; set; }

        [Required]
        public AppointmentStatus Status { get; set; } // Passed or Failed

        public string? DoctorNotes { get; set; }
    }
}
