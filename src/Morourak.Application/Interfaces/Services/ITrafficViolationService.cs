using Morourak.Application.DTOs.Violations;
using Morourak.Domain.Enums.Violations;

namespace Morourak.Application.Interfaces.Services
{
    public interface ITrafficViolationService
    {
        // Query
        Task<ViolationListResponseDto> GetViolationsByLicenseNumberAsync(string licenseNumber, LicenseType licenseType);
        Task<ViolationDetailsDto> GetViolationDetailsAsync(string licenseNumber, LicenseType licenseType);

        // Payment
        Task<PaymentResultDto> PaySingleViolationAsync(int violationId, decimal amount);
        Task<PaymentResultDto> PaySelectedViolationsAsync(List<int> violationIds);
        Task<PaymentResultDto> PayAllViolationsAsync(string licenseNumber, LicenseType licenseType);
    }
}