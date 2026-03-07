using Microsoft.AspNetCore.Http;
using Morourak.Domain.Enums.Vehicles;

namespace Morourak.API.DTOs.VehicleLicenses
{
    public class UploadVehicleLicenseDocumentsApiDto
    {
        public VehicleType VehicleType { get; set; }
        public string Brand { get; set; } = null!;
        public string Model { get; set; } = null!;

        public IFormFile OwnershipProof { get; set; } = null!;
        public IFormFile VehicleDataCertificate { get; set; } = null!;
        public IFormFile IdCard { get; set; } = null!;
        public IFormFile InsuranceCertificate { get; set; } = null!;
        public IFormFile? CustomClearance { get; set; }
    }
}