using Morourak.Domain.Common;
using Morourak.Domain.Enums.Common;

namespace Morourak.Domain.Entities
{
    public class DeliveryRequest : BaseEntity<int>
    {
        public DeliveryMethod DeliveryMethod { get; set; }

        public Address? DeliveryAddress { get; set; }

        public int ReferenceId { get; set; }     // ApplicationId / LicenseId
        public string ReferenceType { get; set; } = null!;
        // مثال: "VehicleLicenseRenewal", "Replacement"

        public DeliveryStatus Status { get; set; } = DeliveryStatus.Pending;

    }
}