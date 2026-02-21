using Morourak.Domain.Enums;
using Morourak.Domain.Enums.Driving;

namespace Morourak.Application.DTOs.DrivingLicenses
{
    public class DrivingLicenseApplicationDto
    {
        public int Id { get; set; }
        public DrivingLicenseCategory Category { get; set; } 
        public string Governorate { get; set; } = null!; 
        public string LicensingUnit { get; set; } = null!;
        public LicenseStatus Status { get; set; }
        public string RequestNumber { get; set; }
    }
}