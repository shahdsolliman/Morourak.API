namespace Morourak.Application.DTOs.Vehicles
{
    public class VehicleLicenseApplicationDto
    {
        public int Id { get; set; }
        public string VehicleType { get; set; } = null!;
        public string Brand { get; set; } = null!;
        public string Model { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string RequestNumber { get; set; } = null!;
    }
}
