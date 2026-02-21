using System;
using System.Collections.Generic;
using Morourak.Domain.Enums.Vehicles;

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
        public int ManufactureYear { get; set; }
        public string Status { get; set; } = null!;
        public string Governorate { get; set; } = null!;
        public DateTime IssueDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string CitizenNationalId { get; set; } = null!;
        public string CitizenName { get; set; } = null!;
        public List<VehicleViolationDto> Violations { get; set; } = new();
    }
}
