using Morourak.Domain.Enums.Driving;

namespace Morourak.API.DTOs.DrivingLicenses
{
    public class SubmitRenewalRequestApiDto
    {
        public DrivingLicenseCategory? NewCategory { get; set; }
        public IFormFile MedicalCertificate { get; set; }
    }
}
