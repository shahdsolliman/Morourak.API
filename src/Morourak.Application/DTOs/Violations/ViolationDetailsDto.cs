using Morourak.Domain.Enums.Violations;

namespace Morourak.Application.DTOs.Violations
{
    /// <summary>
    /// Full violation details DTO for the "View Details" screen.
    /// </summary>
    public class ViolationDetailsDto
    {
        public int ViolationId { get; set; }
        public string ViolationNumber { get; set; } = null!;
        public string CitizenName { get; set; } = null!;
        public string NationalId { get; set; } = null!;
        public string LicenseType { get; set; } = null!;
        public string LicenseTypeAr { get; set; } = null!;
        public int RelatedLicenseId { get; set; }
        public string ViolationType { get; set; } = null!;
        public string ViolationTypeAr { get; set; } = null!;
        public string LegalReference { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string ViolationDateTime { get; set; }
        public decimal FineAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount => FineAmount - PaidAmount;
        public ViolationStatus Status { get; set; }
        public string StatusAr { get; set; } = null!;
        public bool IsPayable { get; set; }
    }
}
