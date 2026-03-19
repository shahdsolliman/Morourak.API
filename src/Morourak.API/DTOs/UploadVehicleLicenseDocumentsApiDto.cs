using Microsoft.AspNetCore.Http;
using Morourak.Domain.Enums.Vehicles;

namespace Morourak.API.DTOs.VehicleLicenses
{
    /// <summary>
    /// Request DTO for uploading vehicle license documents (API layer, includes IFormFile).
    /// </summary>
    public class UploadVehicleLicenseDocumentsApiDto
    {
        /// <summary>
        /// Type of vehicle.
        /// </summary>
        public VehicleType VehicleType { get; set; }

        /// <summary>
        /// Vehicle brand.
        /// </summary>
        public string Brand { get; set; } = null!;

        /// <summary>
        /// Vehicle model.
        /// </summary>
        public string Model { get; set; } = null!;

        /// <summary>
        /// Ownership proof file.
        /// </summary>
        public IFormFile OwnershipProof { get; set; } = null!;

        /// <summary>
        /// Vehicle data certificate file.
        /// </summary>
        public IFormFile VehicleDataCertificate { get; set; } = null!;

        /// <summary>
        /// Owner's ID card file.
        /// </summary>
        public IFormFile IdCard { get; set; } = null!;

        /// <summary>
        /// Insurance certificate file.
        /// </summary>
        public IFormFile InsuranceCertificate { get; set; } = null!;

        /// <summary>
        /// Optional custom clearance document file.
        /// </summary>
        public IFormFile? CustomClearance { get; set; }
    }
}