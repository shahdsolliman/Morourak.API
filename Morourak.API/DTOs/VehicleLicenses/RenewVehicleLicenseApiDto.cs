using System.ComponentModel.DataAnnotations;

namespace Morourak.API.DTOs.VehicleLicenses
{
    /// <summary>
    /// Request DTO for renewing a vehicle license.
    /// </summary>
    public class RenewVehicleLicenseApiDto
    {
        /// <summary>
        /// The current vehicle license number to be renewed.
        /// </summary>
        /// <example>V123456</example>
        [Required]
        public string VehicleLicenseNumber { get; set; } = null!;
    }
}
