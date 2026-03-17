using Morourak.Domain.Enums.Violations;

namespace Morourak.Application.DTOs.Violations
{
    /// <summary>
    /// Summary DTO for violation list views.
    /// </summary>
    /// <summary>
    /// Data transfer object representing a traffic violation.
    /// </summary>
    public class ViolationDto
    {
        /// <summary>Internal unique identifier for the violation.</summary>
        public int ViolationId { get; set; }

        /// <summary>Official violation number or code.</summary>
        public string ViolationNumber { get; set; } = null!;

        /// <summary>Category or type of the violation (e.g., Speeding).</summary>
        public string ViolationType { get; set; } = null!;

        /// <summary>The legal article or law referenced by this violation.</summary>
        public string LegalReference { get; set; } = null!;

        /// <summary>Detailed description of the violation event.</summary>
        public string Description { get; set; } = null!;

        /// <summary>Physical location where the violation occurred.</summary>
        public string Location { get; set; } = null!;

        /// <summary>Date and time the violation was recorded.</summary>
        public string ViolationDateTime { get; set; } = null!;

        /// <summary>Total fine amount in EGP.</summary>
        public decimal FineAmount { get; set; }

        /// <summary>Amount already paid towards this fine.</summary>
        public decimal PaidAmount { get; set; }

        /// <summary>Remaining amount to be paid.</summary>
        public decimal RemainingAmount => FineAmount - PaidAmount;

        /// <summary>Status of the violation (e.g., Unpaid, Paid).</summary>
        public ViolationStatus Status { get; set; }

        /// <summary>Status in Arabic.</summary>
        public string StatusAr { get; set; } = null!;

        /// <summary>Indicates if the violation can be paid online.</summary>
        public bool IsPayable { get; set; }
    }
}
