using Morourak.Domain.Common;
using Morourak.Domain.Enums;
using Morourak.Domain.Enums.Common;
using Morourak.Domain.Enums.Vehicles;

namespace Morourak.Domain.Entities
{
    public class VehicleLicense : BaseEntity<int>
    {
        public string VehicleLicenseNumber { get; set; } = null!;
        public int CitizenRegistryId { get; set; }
        public CitizenRegistry? Citizen { get; set; }
        public string PlateNumber { get; set; } = null!;
        public VehicleType VehicleType { get; set; }
        public string Brand { get; set; } = null!;
        public string Model { get; set; } = null!;
        public DateTime IssueDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        
        public bool IsReplaced { get; set; } = false;
        public bool IsPendingRenewal { get; set; } = false;

        public LicenseStatus Status
        {
            get
            {
                if (IsReplaced) return LicenseStatus.Replaced;
                if (IsPendingRenewal) return LicenseStatus.PendingRenewal;
                if (DateTime.UtcNow > ExpiryDate) return LicenseStatus.Expired;
                return LicenseStatus.Active;
            }
        }


        public string ChassisNumber { get; set; }
        public string EngineNumber { get; set; }


        public int? ExaminationId { get; set; }

        public Appointment? Examination { get; set; }

        public DeliveryMethod DeliveryMethod { get; set; }
        public Address? DeliveryAddress { get; set; }

        public ICollection<VehicleViolation> Violations { get; set; } = new List<VehicleViolation>();
    }
}
