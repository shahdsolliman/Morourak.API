using Morourak.Domain.Enums;
using Morourak.Domain.Enums.Driving;

namespace Morourak.Application.DTOs.DrivingLicenses
{
    public class UploadDrivingLicenseDocumentsDto
    {
        public byte[] PersonalPhoto { get; set; } = null!;
        public byte[] EducationalCertificate { get; set; } = null!;
        public byte[] IdCard { get; set; } = null!;
        public byte[] ResidenceProof { get; set; } = null!;
        public byte[] MedicalCertificate { get; set; } = null!;

        public DrivingLicenseCategory Category { get; set; }
        public string Governorate { get; set; } = null!;
        public string LicensingUnit { get; set; } = null!;

        public LicenseStatus Status { get; set; }
    }
}