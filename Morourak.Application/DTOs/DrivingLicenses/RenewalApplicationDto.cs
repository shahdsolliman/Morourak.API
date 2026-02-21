using Morourak.Domain.Enums;

namespace Morourak.Application.DTOs.DrivingLicenses
{
    public class RenewalApplicationDto
    {
        public int Id { get; set; }
        public string DrivingLicenseNumber { get; set; }
        public string CurrentCategory { get; set; }
        public string RequestedCategory { get; set; }
        public LicenseStatus Status { get; set; }
        public string RequestNumber { get; set; }
    }
}
