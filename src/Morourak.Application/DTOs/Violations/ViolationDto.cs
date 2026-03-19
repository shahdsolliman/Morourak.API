using Morourak.Domain.Enums.Violations;

namespace Morourak.Application.DTOs.Violations
{
    /// <summary>
    /// Data transfer object representing a traffic violation.
    /// </summary>
    public class ViolationDto
    {
        public int ViolationId { get; set; }
        public string ViolationNumber { get; set; } = null!;
        public string ViolationType { get; set; } = null!;
        public string LegalReference { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string ViolationDateTime { get; set; } = null!;
        public decimal FineAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount => FineAmount - PaidAmount;
        public ViolationStatus Status { get; set; }
        public string StatusAr { get; set; } = null!;
        public bool IsPayable { get; set; }
    }
}
