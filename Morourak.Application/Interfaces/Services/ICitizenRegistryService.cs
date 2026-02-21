namespace Morourak.Application.Interfaces.Services
{
    /// <summary>
    /// Provides validation logic against the mock governmental citizen registry.
    /// Used during user registration to verify National ID ownership.
    /// </summary>
    public interface ICitizenRegistryService
    {
        /// <summary>
        /// Validates that the given National ID exists
        /// and the provided mobile number is linked to it.
        /// </summary>
        Task<(bool IsValid, string Message)> ValidateAsync(
            string nationalId,
            string mobileNumber);

        /// <summary>
        /// Gets the CitizenRegistry Id for the given National ID, or null if not found.
        /// </summary>
        Task<int?> GetCitizenIdByNationalIdAsync(string nationalId);
    }
}
