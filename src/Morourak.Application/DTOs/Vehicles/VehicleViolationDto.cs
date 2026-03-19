using System;

namespace Morourak.Application.DTOs.Vehicles
{
    /// <summary>
    /// Data transfer object for a violation associated with a specific vehicle.
    /// </summary>
    public class VehicleViolationDto
    {
        /// <summary>Internal identifier.</summary>
        public int Id { get; set; }

        /// <summary>Date the violation occurred.</summary>
        public DateTime ViolationDate { get; set; }

        /// <summary>Type of the violation.</summary>
        public string ViolationType { get; set; } = null!;

        /// <summary>Fine amount for this specific violation.</summary>
        public decimal Amount { get; set; }

        /// <summary>Detailed description of the violation.</summary>
        public string Description { get; set; } = null!;

        /// <summary>Indicates if this specific violation has been paid.</summary>
        public bool IsPaid { get; set; }
    }
}
