using Morourak.Application.DTOs.Delivery;

namespace Morourak.Application.DTOs.Vehicles
{
    public class VehicleLicenseResponseDto
    {
        public int Id { get; set; }
        public string VehicleLicenseNumber { get; set; } = null!;
        public string PlateNumber { get; set; } = null!;
        public string VehicleType { get; set; } = null!;
        public string Brand { get; set; } = null!;
        public string Model { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateOnly IssueDate { get; set; }
        public DateOnly ExpiryDate { get; set; }
        public string CitizenNationalId { get; set; } = null!;
        public string CitizenName { get; set; } = null!;
        public DeliveryInfoDto Delivery { get; set; } = null!;
    }
}
