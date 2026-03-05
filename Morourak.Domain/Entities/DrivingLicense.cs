using Morourak.Domain.Common;
using Morourak.Domain.Enums;
using Morourak.Domain.Enums.Common;
using Morourak.Domain.Enums.Driving;

namespace Morourak.Domain.Entities
{
    public class DrivingLicense : BaseEntity<int>
    {
        public string LicenseNumber { get; set; } = string.Empty;

        public DrivingLicenseCategory Category { get; set; }

        public int CitizenRegistryId { get; set; }

        public CitizenRegistry? Citizen { get; set; }

        public string LicensingUnit { get; set; } = string.Empty;

        public string Governorate { get; set; } = string.Empty;

        public DateOnly IssueDate { get; set; }

        public DateOnly ExpiryDate { get; set; }

        public ICollection<DrivingLicenseApplication> Applications { get; set; }
            = new List<DrivingLicenseApplication>();


        public DeliveryMethod DeliveryMethod { get; set; }
        public Address? DeliveryAddress { get; set; }

        public bool IsReplaced { get; set; } = false;

        public bool IsPendingRenewal { get; set; } = false;

        public LicenseStatus Status
        {
            get
            {
                if (IsReplaced) return LicenseStatus.Replaced;
                if (IsPendingRenewal) return LicenseStatus.PendingRenewal;
                if (DateOnly.FromDateTime(DateTime.UtcNow) > ExpiryDate) return LicenseStatus.Expired;
                return LicenseStatus.Active;
            }
        }
    }
}