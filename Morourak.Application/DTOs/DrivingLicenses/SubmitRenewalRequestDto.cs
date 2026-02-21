using Morourak.Domain.Enums.Driving;

namespace Morourak.Application.DTOs.DrivingLicenses
{
    public class SubmitRenewalRequestDto
    {
        public DrivingLicenseCategory? NewCategory { get; set; }
        public byte[] MedicalCertificate { get; set; }
    }
}
