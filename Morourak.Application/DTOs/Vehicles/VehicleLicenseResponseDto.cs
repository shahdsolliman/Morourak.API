using Morourak.Application.DTOs.Delivery;

namespace Morourak.Application.DTOs.Vehicles
{
    /// <summary>
    /// Response containing full details of a vehicle license.
    /// </summary>
    public class VehicleLicenseResponseDto
    {
        /// <summary>
        /// Internal identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Official vehicle license number.
        /// </summary>
        public string VehicleLicenseNumber { get; set; } = null!;

        /// <summary>
        /// Vehicle license plate number.
        /// </summary>
        public string PlateNumber { get; set; } = null!;

        /// <summary>
        /// Vehicle type.
        /// </summary>
        public string VehicleType { get; set; } = null!;

        /// <summary>
        /// Vehicle brand.
        /// </summary>
        public string Brand { get; set; } = null!;

        /// <summary>
        /// Vehicle model.
        /// </summary>
        public string Model { get; set; } = null!;

        /// <summary>
        /// License status.
        /// </summary>
        public string Status { get; set; } = null!;

        /// <summary>
        /// Date of issuance.
        /// </summary>
        public DateOnly IssueDate { get; set; }

        /// <summary>
        /// Date of expiration.
        /// </summary>
        public DateOnly ExpiryDate { get; set; }

        /// <summary>
        /// Owner's national ID.
        /// </summary>
        public string CitizenNationalId { get; set; } = null!;

        /// <summary>
        /// Owner's full name.
        /// </summary>
        public string CitizenName { get; set; } = null!;

        /// <summary>
        /// Delivery details for the physical license.
        /// </summary>
        public DeliveryInfoDto Delivery { get; set; } = null!;
    }
}
