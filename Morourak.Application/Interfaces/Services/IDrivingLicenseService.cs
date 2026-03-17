using Morourak.Application.DTOs.Delivery;
using Morourak.Application.DTOs.DrivingLicenses;
using Morourak.Application.DTOs.Licenses;
using Morourak.Domain.Entities;
using Morourak.Domain.Enums.Appointments;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Morourak.Application.Interfaces
{
    public interface IDrivingLicenseService
    {
        #region First-Time Process

        Task<DrivingLicenseApplicationDto> UploadInitialDocumentsAsync(
            string nationalId,
            UploadDrivingLicenseDocumentsDto dto);


        Task<Morourak.Application.DTOs.ServiceRequestDto> FinalizeLicenseAsync(string requestNumber, string nationalId, DeliveryInfoDto delivery);

        #endregion

        #region Renew License

        Task<RenewalApplicationDto> SubmitRenewalRequestAsync(
    string nationalId,
    SubmitRenewalRequestDto dto);

        Task<Morourak.Application.DTOs.ServiceRequestDto> FinalizeRenewalAsync(
            string requestNumber,
            string nationalId,
            DeliveryInfoDto delivery);

        #endregion

        #region Replacement

        Task<Morourak.Application.DTOs.ServiceRequestDto> IssueReplacementAsync(
            string nationalId,
            string drivingLicenseNumber,
            string replacementType,
            DeliveryInfoDto delivery);

        #endregion

        #region Issuance

        Task<DrivingLicenseResponseDto> CompleteIssuanceAsync(string requestNumber);

        #endregion

        #region Queries

        Task<IEnumerable<DrivingLicenseDto>> GetAllLicensesByCitizenAsync(
            string nationalId);

        #endregion

        Task<DrivingLicenseApplication> GetApplicationByIdAsync(int applicationId, string nationalId);

        // BUG 3 FIX: Exposed so ExaminatorController can submit driving exam results
        Task SubmitAppointmentResultAsync(int applicationId, AppointmentType type, bool passed, string? notes);

    }
}
