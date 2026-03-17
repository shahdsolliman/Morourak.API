using Microsoft.AspNetCore.Http;
using Morourak.Application.DTOs.Delivery;
using Morourak.Domain.Enums;
using Morourak.Domain.Enums.Common;
using Morourak.Domain.Enums.Driving;

namespace Morourak.API.DTOs.DrivingLicenses
{
    /// <summary>
    /// Data required to apply for a first-time driving license (API layer, includes IFormFile for uploads).
    /// </summary>
    public class IssueNewDrivingLicenseApiDto
    {
        /// <summary>
        /// Personal photo of the applicant.
        /// </summary>
        public IFormFile PersonalPhoto { get; set; } = null!;

        /// <summary>
        /// Scanned educational certificate.
        /// </summary>
        public IFormFile EducationalCertificate { get; set; } = null!;

        /// <summary>
        /// Scanned national ID card.
        /// </summary>
        public IFormFile IdCard { get; set; } = null!;

        /// <summary>
        /// Scanned proof of residence.
        /// </summary>
        public IFormFile ResidenceProof { get; set; } = null!;

        /// <summary>
        /// Scanned medical certificate.
        /// </summary>
        public IFormFile MedicalCertificate { get; set; } = null!;

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
        /// Preferred delivery method.
        /// </summary>
        public DeliveryMethod DeliveryMethod { get; set; } 

        /// <summary>
        /// Detailed delivery address if applicable.
        /// </summary>
        public AddressDto? DeliveryAddress { get; set; } = null;
    }
}