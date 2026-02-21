using Morourak.Domain.Enums;
using Morourak.Domain.Enums.Vehicles;

namespace Morourak.Application.DTOs.Vehicles
{
    public class VehicleLicenseApplicationDto
    {
        public int Id { get; set; }
        public VehicleType VehicleType { get; set; }
        public string Brand { get; set; } = null!;
        public string Model { get; set; } = null!;
        public int ManufactureYear { get; set; }
        public string Governorate { get; set; } = null!;
        public LicenseStatus Status { get; set; }
        public string RequestNumber { get; set; } = null!;
    }
}
