using System;

namespace Morourak.Application.DTOs.Vehicles
{
    public class VehicleViolationDto
    {
        public int Id { get; set; }
        public DateTime ViolationDate { get; set; }
        public string ViolationType { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Description { get; set; } = null!;
        public bool IsPaid { get; set; }
    }
}
