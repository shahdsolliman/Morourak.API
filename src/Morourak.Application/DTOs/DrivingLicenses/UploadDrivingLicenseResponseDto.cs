using Morourak.Domain.Enums;
using Morourak.Domain.Enums.Driving;

namespace Morourak.Application.DTOs.DrivingLicenses
{
    public class UploadDrivingLicenseResponseDto
    {
        public int ApplicationId { get; set; }
        public string CitizenNationalId { get; set; } = null!;
        public DrivingLicenseCategory Category { get; set; }
        public LicenseStatus Status { get; set; }
        public DateTime SubmittedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}