using Morourak.Domain.Enums;
using Morourak.Domain.Enums.Driving;

namespace Morourak.Application.DTOs.DrivingLicenses
{
    public class DrivingLicenseApplicationDto
    {
        public int Id { get; set; }
        public string Category { get; set; } 
        public string Governorate { get; set; } = null!; 
        public string LicensingUnit { get; set; } = null!;
        public string Status { get; set; }
        public string RequestNumber { get; set; }
    }
}