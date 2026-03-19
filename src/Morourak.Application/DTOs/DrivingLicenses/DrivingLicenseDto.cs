using Morourak.Domain.Enums;
using Morourak.Domain.Enums.Driving;

namespace Morourak.Application.DTOs.Licenses
{
    /// <summary>
    /// Represents a driving license issued to a citizen.
    /// </summary>
    public class DrivingLicenseDto
    {
        public string LicenseNumber { get; set; } = null!;
        public string Category { get; set; } = default!;
        public string Status { get; set; } = default!;
        public string CitizenNationalId { get; set; } = null!;
        public string LicensingUnit { get; set; } = null!;
        public string Governorate { get; set; } = null!;
        public DateOnly IssueDate { get; set; }
        public DateOnly ExpiryDate { get; set; }
    }
}