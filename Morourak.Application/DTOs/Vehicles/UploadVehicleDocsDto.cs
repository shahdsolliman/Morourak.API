using Morourak.Domain.Enums.Vehicles;

namespace Morourak.Application.DTOs.Vehicles
{
    /// <summary>
    /// Data transfer object for uploading vehicle-related documents for license applications or renewals.
    /// </summary>
    public class UploadVehicleDocsDto
    {
        /// <summary>
        /// Type of the vehicle.
        /// </summary>
        public VehicleType VehicleType { get; set; }

        /// <summary>
        /// Vehicle manufacturer brand.
        /// </summary>
        /// <example>Toyota</example>
        public string Brand { get; set; } = null!;

        /// <summary>
        /// Vehicle model name.
        /// </summary>
        /// <example>Corolla</example>
        public string Model { get; set; } = null!;

        /// <summary>
        /// Existing vehicle license number (required for renewals).
        /// </summary>
        public string? VehicleLicenseNumber { get; set; }

        /// <summary>
        /// Ownership proof document (as byte array).
        /// </summary>
        public byte[]? OwnershipProof { get; set; }

        /// <summary>
        /// Vehicle data certificate (as byte array).
        /// </summary>
        public byte[]? VehicleDataCertificate { get; set; }

        /// <summary>
        /// Owner's ID card (as byte array).
        /// </summary>
        public byte[]? IdCard { get; set; }

        /// <summary>
        /// Insurance certificate (as byte array).
        /// </summary>
        public byte[]? InsuranceCertificate { get; set; }

        /// <summary>
        /// Custom clearance document (as byte array, optional).
        /// </summary>
        public byte[]? CustomClearance { get; set; }
    }
}
