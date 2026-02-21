using Morourak.Application.DTOs.Delivery;
using Morourak.Application.DTOs.DrivingLicenses;
using Morourak.Application.DTOs.Licenses;
using Morourak.Domain.Entities;
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


        Task<DrivingLicenseResponseDto> FinalizeLicenseAsync(string requestNumber, string nationalId, DeliveryInfoDto delivery);

        #endregion

        #region Renew License

        Task<RenewalApplicationDto> SubmitRenewalRequestAsync(
    string nationalId,
    SubmitRenewalRequestDto dto);

        Task<DrivingLicenseResponseDto> FinalizeRenewalAsync(
            string requestNumber,
            string nationalId,
            DeliveryInfoDto delivery);

        #endregion

        #region Replacement

        Task<DrivingLicenseResponseDto> IssueReplacementAsync(
            string nationalId,
            string drivingLicenseNumber,
            string replacementType,
            DeliveryInfoDto delivery);

        #endregion

        #region Queries

        Task<IEnumerable<DrivingLicenseDto>> GetAllLicensesByCitizenAsync(
            string nationalId);

        #endregion

        Task<DrivingLicenseApplication> GetApplicationByIdAsync(int applicationId, string nationalId);

    }
}