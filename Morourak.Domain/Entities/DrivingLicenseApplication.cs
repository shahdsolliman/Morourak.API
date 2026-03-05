using Morourak.Domain.Common;
using Morourak.Domain.Enums;
using Morourak.Domain.Enums.Driving;

namespace Morourak.Domain.Entities
{
    public class DrivingLicenseApplication : BaseEntity<int>
    {
        public DrivingLicenseCategory Category { get; set; }

        public string Governorate { get; set; } = string.Empty;
        public string LicensingUnit { get; set; } = string.Empty;

        public string PersonalPhotoPath { get; set; } = null!;
        public string EducationalCertificatePath { get; set; } = null!;
        public string IdCardPath { get; set; } = null!;
        public string ResidenceProofPath { get; set; } = null!;

        public bool MedicalExaminationPassed { get; set; } = false;
        public bool DrivingTestPassed { get; set; } = false;

        public int? DrivingLicenseId { get; set; }
        public DrivingLicense? DrivingLicense { get; set; }

        public LicenseStatus Status { get; set; } = LicenseStatus.Pending;
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public int CitizenRegistryId { get; set; }   
        public CitizenRegistry? Citizen { get; set; }
    }
}