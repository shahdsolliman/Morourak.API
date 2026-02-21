using Morourak.Application.DTOs.Delivery;
using Morourak.Domain.Enums.Common;
using Morourak.Domain.Enums.Vehicles;

namespace Morourak.API.DTOs.VehicleLicenses
{
    public class IssueNewLicenseApiDto
    {
        public IFormFile NationalID { get; set; }
        public IFormFile OwnershipProof { get; set; }
        public IFormFile VehicleDataCertificate { get; set; }
        public IFormFile InsuranceCertificate { get; set; }
        public IFormFile? CustomClearance { get; set; }
        public IFormFile? TechnicalInspectionReceipt { get; set; }

        public string PlateNumber { get; set; }
        public VehicleType VehicleType { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public int ManufactureYear { get; set; }

        public DeliveryMethod DeliveryMethod { get; set; } // eg. "Pickup" / "Home Delivery"
        public AddressDto? DeliveryAddress { get; set; }
    }
}