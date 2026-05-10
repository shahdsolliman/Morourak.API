using Morourak.Application.DTOs.DrivingLicenses;
using Morourak.Application.DTOs;
using Morourak.Application.DTOs.Delivery;

namespace Morourak.Application.Interfaces.Services
{
    public interface IDrivingLicenseIssuanceService
    {
        Task<DrivingLicenseApplicationDto> UploadInitialDocumentsAsync(string nationalId, UploadDrivingLicenseDocumentsDto dto);
        Task<ServiceRequestDto> FinalizeLicenseAsync(string requestNumber, string nationalId, DeliveryInfoDto delivery);
        Task<DrivingLicenseResponseDto> CompleteIssuanceAsync(string requestNumber);
    }
}
