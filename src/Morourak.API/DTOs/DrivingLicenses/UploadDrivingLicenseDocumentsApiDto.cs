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
        /// Scanned proof of residence file (optional).
        /// </summary>
        public IFormFile? ResidenceProof { get; set; }

        /// <summary>
        /// Scanned medical certificate file (optional). 
        /// If provided, medical examination booking is not required.
        /// </summary>
        public IFormFile? MedicalCertificate { get; set; }
    }
}