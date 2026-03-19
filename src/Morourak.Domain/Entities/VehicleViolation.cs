using Morourak.Domain.Common;

namespace Morourak.Domain.Entities
{
    public class VehicleViolation : BaseEntity<int>
    {
        public int VehicleLicenseId { get; set; }
        public VehicleLicense VehicleLicense { get; set; } = null!;

        public DateTime ViolationDate { get; set; }
        public string ViolationType { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Description { get; set; } = null!;
        public bool IsPaid { get; set; } = false;
    }
}
