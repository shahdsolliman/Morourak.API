using Morourak.Application.DTOs.Auth;

namespace Morourak.Application.Interfaces.Services
{
    public interface ICitizenRegistryService
    {
        /// <summary>
        /// Validates full citizen data against registry.
        /// </summary>
        Task<CitizenMatchResult> ValidateFullMatchAsync(
            string nationalId,
            string firstName,
            string lastName,
            string mobileNumber);

        /// <summary>
        /// Gets the CitizenRegistry Id for the given National ID, or null if not found.
        /// </summary>
        Task<int?> GetCitizenIdByNationalIdAsync(string nationalId);
    }
}