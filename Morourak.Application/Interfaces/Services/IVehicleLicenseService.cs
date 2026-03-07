using Morourak.Application.DTOs.Delivery;
using Morourak.Application.DTOs.Vehicles;
using Morourak.Domain.Entities;

namespace Morourak.Application.Interfaces
{
    public interface IVehicleLicenseService
    {
        #region First-Time Process

        Task<VehicleLicenseApplicationDto> UploadInitialDocumentsAsync(
            string nationalId,
            UploadVehicleDocsDto dto);

        Task<VehicleLicenseResponseDto> FinalizeLicenseAsync(
            string requestNumber,
            string nationalId,
            DeliveryInfoDto delivery);

        #endregion

        #region Renew License

        Task<VehicleLicenseApplicationDto> SubmitRenewalRequestAsync(
            string nationalId,
            UploadVehicleDocsDto dto);

        Task<VehicleLicenseResponseDto> FinalizeRenewalAsync(
            string requestNumber,
            string nationalId,
            DeliveryInfoDto delivery);

        #endregion

        #region Replacement

        Task<VehicleLicenseResponseDto> IssueReplacementAsync(
            string nationalId,
            string vehicleLicenseNumber,
            string replacementType,
            DeliveryInfoDto delivery);

        #endregion

        #region Queries

        Task<VehicleLicenseApplication?> GetApplicationByIdAsync(int id, string nationalId);

        Task<IEnumerable<VehicleLicenseDto>> GetAllLicensesByCitizenAsync(string nationalId);

        Task<IEnumerable<VehicleTypeDetailDto>> GetVehicleTypesAsync();

        #endregion
    }
}
