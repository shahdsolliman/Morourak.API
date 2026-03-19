using Morourak.Domain.Enums.Appointments;
using System.ComponentModel.DataAnnotations;

namespace Morourak.Application.DTOs.Appointments
{
    /// <summary>
    /// Request DTO for updating the status of an examination appointment.
    /// </summary>
    public class UpdateExaminationStatusDto
    {
        /// <summary>Internal identifier of the examination to update.</summary>
        [Required]
        public int ExaminationId { get; set; }

        /// <summary>The new status to be applied.</summary>
        [Required]
        public AppointmentStatus NewStatus { get; set; }
    }
}
