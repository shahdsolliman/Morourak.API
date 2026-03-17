using System;

namespace Morourak.Application.DTOs.Vehicles
{
    /// <summary>
    /// Represents a vehicle license issued to a citizen.
    /// </summary>
    public class VehicleLicenseDto
    {
        /// <summary>
        /// Internal unique identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Unique vehicle license number.
        /// </summary>
        public string VehicleLicenseNumber { get; set; } = null!;

        /// <summary>
        /// The vehicle's license plate number.
        /// </summary>
        public string PlateNumber { get; set; } = null!;

        /// <summary>
        /// Type of vehicle (e.g., Car, Motorcycle).
        /// </summary>
        public string VehicleType { get; set; } = null!;

        /// <summary>
        /// Vehicle manufacturer brand.
        /// </summary>
        public string Brand { get; set; } = null!;

        /// <summary>
        /// Vehicle model name.
        /// </summary>
        public string Model { get; set; } = null!;

        /// <summary>
        /// Current status of the license.
        /// </summary>
        public string Status { get; set; } = null!;

        /// <summary>
        /// Date the license was issued.
        /// </summary>
        public DateTime IssueDate { get; set; }

        /// <summary>
        /// Date the license expires.
        /// </summary>
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// National ID of the vehicle owner.
        /// </summary>
        public string CitizenNationalId { get; set; } = null!;
    }
}
