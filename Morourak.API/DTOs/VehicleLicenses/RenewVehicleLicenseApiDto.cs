using System.ComponentModel.DataAnnotations;

namespace Morourak.API.DTOs.VehicleLicenses
{
    public class RenewVehicleLicenseApiDto
    {
        [Required]
        public string VehicleLicenseNumber { get; set; } = null!;

       
    }
}
