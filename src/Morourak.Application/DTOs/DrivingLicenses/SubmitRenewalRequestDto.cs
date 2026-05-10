using Morourak.Domain.Enums.Driving;

namespace Morourak.Application.DTOs.DrivingLicenses
{
    public class SubmitRenewalRequestDto
    {
        public string LicenseNumber { get; set; } = string.Empty;
        public DrivingLicenseCategory? NewCategory { get; set; }
    }
}
