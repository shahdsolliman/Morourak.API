using Morourak.Application.DTOs.DrivingLicenses;
using Morourak.Application.DTOs;
using Morourak.Application.DTOs.Delivery;

namespace Morourak.Application.Interfaces.Services
{
    public interface IDrivingLicenseRenewalService
    {
        Task<RenewalApplicationDto> SubmitRenewalRequestAsync(string nationalId, SubmitRenewalRequestDto dto);
        Task<ServiceRequestDto> FinalizeRenewalAsync(string requestNumber, string nationalId, DeliveryInfoDto delivery);
        Task<DrivingLicenseResponseDto> CompleteRenewalAsync(string requestNumber);
    }
}
