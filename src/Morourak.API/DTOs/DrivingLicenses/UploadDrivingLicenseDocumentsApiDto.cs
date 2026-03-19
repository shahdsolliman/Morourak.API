using Microsoft.AspNetCore.Http;
using Morourak.Domain.Enums;
using Morourak.Domain.Enums.Driving;

namespace Morourak.API.DTOs.DrivingLicenses
{
    /// <summary>
    /// Request DTO for uploading documents for a driving license application (API layer).
    /// </summary>
    public class UploadDrivingLicenseDocumentsApiDto
    {
        /// <summary>
        /// Requested driving license category.
        /// </summary>
        public DrivingLicenseCategory Category { get; set; }

        /// <summary>
        /// Scanned educational certificate file.
        /// </summary>
        public IFormFile EducationalCertificate { get; set; } = null!;

        /// <summary>
        /// Personal photo file.
        /// </summary>
        public IFormFile PersonalPhoto { get; set; } = null!;

        /// <summary>
        /// Scanned national ID card file.
        /// </summary>
        public IFormFile IdCard { get; set; } = null!;

        /// <summary>
        /// Scanned proof of residence file.
        /// </summary>
        public IFormFile ResidenceProof { get; set; } = null!;

        /// <summary>
        /// Selected governorate name.
        /// </summary>
        /// <example>Cairo</example>
        public string Governorate { get; set; } = null!;

        /// <summary>
        /// Selected traffic unit name.
        /// </summary>
        /// <example>Nasr City Unit</example>
        public string LicensingUnit { get; set; } = null!;
    }
}