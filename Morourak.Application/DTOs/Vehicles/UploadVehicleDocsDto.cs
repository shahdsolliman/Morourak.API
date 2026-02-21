using Morourak.Domain.Enums.Vehicles;

namespace Morourak.Application.DTOs.Vehicles
{
    public class UploadVehicleDocsDto
    {
        public VehicleType VehicleType { get; set; }
        public string Brand { get; set; } = null!;
        public string Model { get; set; } = null!;
        public int ManufactureYear { get; set; }
        public string Governorate { get; set; } = null!;
        public string? VehicleLicenseNumber { get; set; }

        public byte[]? OwnershipProof { get; set; }
        public byte[]? VehicleDataCertificate { get; set; }
        public byte[]? IdCard { get; set; }
        public byte[]? InsuranceCertificate { get; set; }
        public byte[]? CustomClearance { get; set; }
    }
}
