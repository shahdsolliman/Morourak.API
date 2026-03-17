using Morourak.Application.DTOs.Delivery;

namespace Morourak.API.DTOs.DrivingLicenses
{
    /// <summary>
    /// Data required to issue a replacement for a driving license.
    /// </summary>
    public class IssueReplacementDrivingLicenseApiDto
    {
        /// <summary>
        /// The reason for replacement (e.g., "Lost" or "Damaged").
        /// </summary>
        /// <example>Lost</example>
        public string ReplacementType { get; set; } = null!;

        /// <summary>
        /// Delivery information and address for the new license.
        /// </summary>
        public DeliveryInfoDto Delivery { get; set; } = null!;
    }
}