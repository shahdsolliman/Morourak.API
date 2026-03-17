using Morourak.Domain.Enums.Driving;

namespace Morourak.API.DTOs.DrivingLicenses
{
    /// <summary>
    /// Request DTO for submitting a driving license renewal request.
    /// </summary>
    public class SubmitRenewalRequestApiDto
    {
        /// <summary>
        /// The new license category requested (if upgrading).
        /// </summary>
        public DrivingLicenseCategory? NewCategory { get; set; }
    }
}
