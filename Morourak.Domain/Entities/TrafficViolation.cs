using Morourak.Domain.Common;
using Morourak.Domain.Enums.Violations;

namespace Morourak.Domain.Entities
{
    /// <summary>
    /// Generic traffic violation linked to either a Driving License or Vehicle License
    /// via the LicenseType discriminator. Supports partial payment tracking.
    /// </summary>
    public class TrafficViolation : BaseEntity<int>
    {
        /// <summary>
        /// FK → CitizenRegistry.Id
        /// </summary>
        public int CitizenRegistryId { get; set; }
        public CitizenRegistry Citizen { get; set; } = null!;

        /// <summary>
        /// Stores DrivingLicense.Id or VehicleLicense.Id depending on LicenseType.
        /// </summary>
        public int RelatedLicenseId { get; set; }

        /// <summary>
        /// Discriminator: Driving or Vehicle.
        /// </summary>
        public LicenseType LicenseType { get; set; }

        public ViolationType ViolationType { get; set; }

        /// <summary>
        /// Unique violation reference number, e.g. "MV-2025-001".
        /// </summary>
        public string ViolationNumber { get; set; } = null!;

        /// <summary>
        /// Arabic legal article reference for UI display.
        /// e.g. "المادة 72 / قانون المرور"
        /// </summary>
        public string LegalReference { get; set; } = null!;

        /// <summary>
        /// Arabic description for UI display.
        /// e.g. "تجاوز السرعة القصوى بمقدار 40 كم/س"
        /// </summary>
        public string Description { get; set; } = null!;

        /// <summary>
        /// Arabic location text.
        /// e.g. "طريق القاهرة - الإسكندرية الصحراوي"
        /// </summary>
        public string Location { get; set; } = null!;

        public DateTime ViolationDateTime { get; set; }

        public decimal FineAmount { get; set; }

        /// <summary>
        /// Amount paid so far. Supports partial payment scenarios.
        /// </summary>
        public decimal PaidAmount { get; set; } = 0;

        public ViolationStatus Status { get; set; } = ViolationStatus.Unpaid;

        /// <summary>
        /// Whether this violation can currently be paid online.
        /// </summary>
        public bool IsPayable { get; set; } = true;
    }
}
