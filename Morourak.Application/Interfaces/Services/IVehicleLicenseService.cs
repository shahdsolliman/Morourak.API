using Morourak.Application.DTOs.Delivery;
using Morourak.Application.DTOs.Vehicles;
using Morourak.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Morourak.Application.Interfaces.Services
{
    public interface IVehicleLicenseService
    {
        Task<VehicleLicenseApplicationDto> UploadInitialDocumentsAsync(string nationalId, UploadVehicleDocsDto dto);
        Task<VehicleLicenseResponseDto> FinalizeLicenseAsync(string requestNumber, string nationalId, DeliveryInfoDto delivery);
        Task<IEnumerable<VehicleLicenseDto>> GetAllLicensesByCitizenAsync(string nationalId);
        Task<IEnumerable<VehicleTypeDetailDto>> GetVehicleTypesAsync();
        Task<IEnumerable<VehicleViolationDto>> GetVehicleViolationsAsync(int vehicleLicenseId);
        Task<VehicleLicenseApplication?> GetApplicationByIdAsync(int id, string nationalId);
        Task<VehicleLicenseApplicationDto> RenewLicenseAsync(string nationalId, UploadVehicleDocsDto dto); // Reusing UploadVehicleDocsDto for simplicity or can use a specialized one
        Task<VehicleLicenseResponseDto> FinalizeRenewalAsync(string requestNumber, string nationalId, DeliveryInfoDto delivery);
        Task<VehicleLicenseResponseDto> IssueReplacementAsync(string nationalId, string vehicleLicenseNumber, string replacementType, DeliveryInfoDto delivery);
    }
}