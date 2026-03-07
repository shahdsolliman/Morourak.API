using Morourak.Domain.Common;
using Morourak.Domain.Enums;
using Morourak.Domain.Enums.Vehicles;

namespace Morourak.Domain.Entities
{
    /// <summary>
    /// Represents a citizen's application for a vehicle license.
    /// Tracks uploaded documents and the application process.
    /// </summary>
    public class VehicleLicenseApplication : BaseEntity<int>
    {
        /// <summary>
        /// FK to the citizen who submitted the application
        /// </summary>
        public int CitizenRegistryId { get; set; }

        /// <summary>
        /// FK to the existing vehicle license (for renewals)
        /// </summary>
        public int? VehicleLicenseId { get; set; }

        /// <summary>
        /// Navigation property to the vehicle license
        /// </summary>
        public VehicleLicense? VehicleLicense { get; set; }

        /// <summary>
        /// Navigation property to the citizen
        /// </summary>
        public CitizenRegistry? Citizen { get; set; }

        #region Vehicle Info

        /// <summary>
        /// Type of the vehicle (PrivateCar, Truck, etc.)
        /// </summary>
        public VehicleType VehicleType { get; set; }

        /// <summary>
        /// Brand of the vehicle
        /// </summary>
        public string Brand { get; set; } = null!;

        /// <summary>
        /// Model of the vehicle
        /// </summary>
        public string Model { get; set; } = null!;

        /// <summary>
        /// Year the vehicle was manufactured
        /// </summary>
        

        #endregion

        #region Uploaded Documents

        public string? OwnershipProofPath { get; set; }
        public string? VehicleDataCertificatePath { get; set; }
        public string? IdCardPath { get; set; }
        public string? InsuranceCertificatePath { get; set; }
        public string? CustomClearancePath { get; set; }

        #endregion

        #region Process Info

        /// <summary>
        /// Indicates if the vehicle passed technical inspection
        /// </summary>
        public bool TechnicalInspectionPassed { get; set; } = false;

        /// <summary>
        /// Current status of the application (Pending, Approved, Rejected)
        /// </summary>
        public LicenseStatus Status { get; set; } = LicenseStatus.Pending;

        /// <summary>
        /// Timestamp when the application was submitted
        /// </summary>
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        #endregion


    }
}