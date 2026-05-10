using System;

namespace Morourak.Application.DTOs.Vehicles
{
    /// <summary>
    /// Represents a vehicle license issued to a citizen.
    /// </summary>
    public class VehicleLicenseDto
    {
        public int Id { get; set; }
        public string VehicleLicenseNumber { get; set; } = null!;
        public string PlateNumber { get; set; } = null!;
        public string VehicleType { get; set; } = null!;
        public string Brand { get; set; } = null!;
        public string Model { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string IssueDate { get; set; } = string.Empty;
        public string ExpiryDate { get; set; } = string.Empty;
        public string CitizenNationalId { get; set; } = null!;
    }
}
