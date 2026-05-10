using Morourak.Application.DTOs.Delivery;
using Morourak.Application.DTOs;
using Morourak.Domain.Enums.Common;

namespace Morourak.Application.Interfaces.Services
{
    public interface IDrivingLicenseReplacementService
    {
        Task<ServiceRequestDto> IssueReplacementAsync(
            string nationalId,
            string drivingLicenseNumber,
            ReplacementType replacementType,
            DeliveryInfoDto delivery);
        Task<DrivingLicenseResponseDto> CompleteReplacementAsync(string requestNumber);
    }
}
