using Morourak.Domain.Enums;
using Morourak.Domain.Enums.Driving;

namespace Morourak.Application.DTOs.DrivingLicenses
{
    /// <summary>
    /// Data required to upload documents for a driving license application.
    /// </summary>
    public class UploadDrivingLicenseDocumentsDto
    {
        /// <summary>
        /// Personal photo of the applicant (as byte array).
        /// </summary>
        public byte[] PersonalPhoto { get; set; } = null!;

        /// <summary>
        /// Scanned educational certificate (as byte array).
        /// </summary>
        public byte[] EducationalCertificate { get; set; } = null!;

        /// <summary>
        /// Scanned national ID card (as byte array).
        /// </summary>
        public byte[] IdCard { get; set; } = null!;

        /// <summary>
        /// Scanned residence proof (as byte array).
        /// </summary>
        public byte[] ResidenceProof { get; set; } = null!;

        /// <summary>
        /// Requested driving license category.
        /// </summary>
        public DrivingLicenseCategory Category { get; set; }

        /// <summary>
        /// Selected governorate.
        /// </summary>
        public string Governorate { get; set; } = null!;

        /// <summary>
        /// Selected traffic unit.
        /// </summary>
        public string LicensingUnit { get; set; } = null!;

        /// <summary>
        /// Initial status of the application.
        /// </summary>
        public LicenseStatus Status { get; set; }
    }
}