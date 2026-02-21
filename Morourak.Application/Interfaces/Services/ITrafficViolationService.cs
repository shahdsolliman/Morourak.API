using Morourak.Application.DTOs.Violations;
using Morourak.Domain.Enums.Violations;

namespace Morourak.Application.Interfaces.Services
{
    /// <summary>
    /// Generic traffic violation service that works for both Driving and Vehicle licenses.
    /// </summary>
    public interface ITrafficViolationService
    {
        /// <summary>
        /// Get all violations for a specific license (Driving or Vehicle).
        /// </summary>
        Task<ViolationListResponseDto> GetViolationsByLicenseAsync(int licenseId, LicenseType licenseType);

        /// <summary>
        /// Get full details of a single violation.
        /// </summary>
        Task<ViolationDetailsDto> GetViolationDetailsAsync(int violationId);

        /// <summary>
        /// Pay a single violation (full or partial amount).
        /// </summary>
        Task<PaymentResultDto> PaySingleViolationAsync(int violationId, decimal amount);

        /// <summary>
        /// Pay multiple selected violations in full.
        /// </summary>
        Task<PaymentResultDto> PaySelectedViolationsAsync(List<int> violationIds);

        /// <summary>
        /// Pay all unpaid violations for a given license.
        /// </summary>
        Task<PaymentResultDto> PayAllViolationsAsync(int licenseId, LicenseType licenseType);
    }
}
