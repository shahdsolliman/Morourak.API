using Morourak.Application.DTOs.DrivingLicenses;
using Morourak.Application.DTOs.Licenses;
using Morourak.Domain.Entities;

namespace Morourak.Application.Interfaces.Services
{
    public interface IDrivingLicenseQueryService
    {
        Task<IEnumerable<DrivingLicenseDto>> GetAllLicensesByCitizenAsync(string nationalId);
        Task<DrivingLicenseApplicationDto> GetApplicationByIdAsync(int applicationId, string nationalId);
        Task<ServiceRequest?> GetRequestByNumberAsync(string requestNumber);
    }
}
