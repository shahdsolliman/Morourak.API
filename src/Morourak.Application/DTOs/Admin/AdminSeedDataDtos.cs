using System;

namespace Morourak.Application.DTOs.Admin
{
    public class CitizenRegistryDto
    {
        public int Id { get; set; }
        public string NationalId { get; set; } = null!;
        public string MobileNumber { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Governorate { get; set; } = null!;
        public string LicensingUnit { get; set; } = null!;
    }

    public class DrivingLicenseDto
    {
        public int Id { get; set; }
        public string LicenseNumber { get; set; } = null!;
        public DateTime IssueDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string CurrentStatus { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string CitizenNationalId { get; set; } = null!;
        public string CitizenName { get; set; } = null!;
    }

    public class VehicleLicenseDto
    {
        public int Id { get; set; }
        public string PlateNumber { get; set; } = null!;
        public string ChassisNumber { get; set; } = null!;
        public string EngineNumber { get; set; } = null!;
        public DateTime IssueDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string CurrentStatus { get; set; } = null!;
        public string VehicleType { get; set; } = null!;
        public string CitizenNationalId { get; set; } = null!;
        public string CitizenName { get; set; } = null!;
    }
}
