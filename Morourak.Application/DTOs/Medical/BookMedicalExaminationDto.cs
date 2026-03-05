using System.ComponentModel.DataAnnotations;

namespace Morourak.Application.DTOs.Medical
{
    public class BookMedicalExaminationDto
    {
        [Required]
        public int ApplicationId { get; set; }

        [Required]
        public string CitizenNationalId { get; set; } = null!;

        [Required]
        public DateTime AppointmentDate { get; set; }
    }
}
