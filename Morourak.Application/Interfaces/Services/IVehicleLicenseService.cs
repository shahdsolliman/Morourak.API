using Morourak.Application.DTOs.Delivery;
using Morourak.Application.DTOs.Vehicles;
using Morourak.Domain.Entities;

namespace Morourak.Application.Interfaces.Services
{
    public interface IVehicleLicenseService
    {
   
        Task<VehicleLicenseApplicationDto> UploadInitialDocumentsAsync(string nationalId, UploadVehicleDocsDto dto);

        Task<VehicleLicenseApplicationDto> RenewLicenseAsync(string nationalId, UploadVehicleDocsDto dto);

        Task<VehicleLicenseResponseDto> FinalizeLicenseAsync(string requestNumber, string nationalId, DeliveryInfoDto delivery);
        Task<VehicleLicenseResponseDto> FinalizeRenewalAsync(string requestNumber, string nationalId, DeliveryInfoDto delivery);

        Task<VehicleLicenseApplication?> GetApplicationByIdAsync(int id, string nationalId);
        Task<IEnumerable<VehicleLicenseDto>> GetAllLicensesByCitizenAsync(string nationalId);

        Task<IEnumerable<VehicleTypeDetailDto>> GetVehicleTypesAsync();

        Task<VehicleLicenseResponseDto> IssueReplacementAsync(string nationalId, string vehicleLicenseNumber, string replacementType, DeliveryInfoDto delivery);
    }
}
