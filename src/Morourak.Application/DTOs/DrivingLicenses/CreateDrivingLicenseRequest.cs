using Morourak.Domain.Enums.Driving;

namespace Morourak.Application.DTOs.Licenses
{
    public class CreateDrivingLicenseRequest
    {
        public string NationalId { get; set; } = null!;

        public DrivingLicenseCategory Category { get; set; }

        public string LicensingUnit { get; set; } = null!;

        public string Governorate { get; set; } = null!;

        public int ExaminationId { get; set; }

        public string DeliveryMethod { get; set; } = null!;

        public string? DeliveryAddress { get; set; }
    }
}