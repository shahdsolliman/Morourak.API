using Morourak.Application.DTOs.Delivery;
using Morourak.Application.DTOs.Vehicles;
using Morourak.Domain.Entities;
using Morourak.Domain.Enums.Appointments;

namespace Morourak.Application.Interfaces
{
    public interface IVehicleLicenseService
    {
        #region First-Time Process

        Task<VehicleLicenseApplicationDto> UploadInitialDocumentsAsync(
            string nationalId,
            UploadVehicleDocsDto dto);

        Task<Morourak.Application.DTOs.ServiceRequestDto> FinalizeLicenseAsync(
            string requestNumber,
            string nationalId,
            DeliveryInfoDto delivery);

        #endregion

        #region Renew License

        Task<VehicleLicenseApplicationDto> SubmitRenewalRequestAsync(
            string nationalId,
            UploadVehicleDocsDto dto);

        Task<Morourak.Application.DTOs.ServiceRequestDto> FinalizeRenewalAsync(
            string requestNumber,
            string nationalId,
            DeliveryInfoDto delivery);

        #endregion

        #region Replacement

        Task<Morourak.Application.DTOs.ServiceRequestDto> IssueReplacementAsync(
            string nationalId,
            string vehicleLicenseNumber,
            string replacementType,
            DeliveryInfoDto delivery);

        Task SubmitAppointmentResultAsync(int applicationId, AppointmentType type, bool passed, string? notes);

        #endregion

        #region Issuance

        Task<VehicleLicenseResponseDto> CompleteIssuanceAsync(string requestNumber);

        #endregion

        #region Queries

        Task<VehicleLicenseApplication?> GetApplicationByIdAsync(int id, string nationalId);

        Task<IEnumerable<VehicleLicenseDto>> GetAllLicensesByCitizenAsync(string nationalId);

        Task<IEnumerable<VehicleTypeDetailDto>> GetVehicleTypesAsync();

        #endregion
    }
}
