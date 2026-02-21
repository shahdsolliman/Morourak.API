using Microsoft.AspNetCore.Http;
using Morourak.Application.DTOs.Delivery;
using Morourak.Domain.Enums;
using Morourak.Domain.Enums.Common;
using Morourak.Domain.Enums.Driving;

namespace Morourak.API.DTOs.DrivingLicenses
{
    public class IssueNewDrivingLicenseApiDto
    {
        // Files
        public IFormFile PersonalPhoto { get; set; } = null!;
        public IFormFile EducationalCertificate { get; set; } = null!;
        public IFormFile IdCard { get; set; } = null!;
        public IFormFile ResidenceProof { get; set; } = null!;
        public IFormFile MedicalCertificate { get; set; } = null!;

        // License data
        public DrivingLicenseCategory Category { get; set; }
        public string Governorate { get; set; } = null!;
        public string LicensingUnit { get; set; } = null!;

        // Delivery info
        public DeliveryMethod DeliveryMethod { get; set; } 
        public AddressDto? DeliveryAddress { get; set; } = null;
    }
}