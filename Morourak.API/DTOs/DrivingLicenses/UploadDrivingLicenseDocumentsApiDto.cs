using Microsoft.AspNetCore.Http;
using Morourak.Domain.Enums;
using Morourak.Domain.Enums.Driving;

namespace Morourak.API.DTOs.DrivingLicenses
{
    public class UploadDrivingLicenseDocumentsApiDto
    {
        public DrivingLicenseCategory Category { get; set; }
        public IFormFile EducationalCertificate { get; set; } = null!;
        public IFormFile PersonalPhoto { get; set; } = null!;
        public IFormFile IdCard { get; set; } = null!;
        public IFormFile ResidenceProof { get; set; } = null!;
        public IFormFile MedicalCertificate { get; set; } = null!; 

        public string Governorate { get; set; } = null!;
        public string LicensingUnit { get; set; } = null!;
    }
}