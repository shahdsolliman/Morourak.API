using Morourak.Application.DTOs.Delivery;

namespace Morourak.API.DTOs.DrivingLicenses
{
    /// <summary>
    /// Data required to issue a replacement for a driving license.
    /// </summary>
    public class IssueReplacementDrivingLicenseApiDto
    {
        /// <summary>
        /// The reason for replacement.
        /// </summary>
        /// <example>بدل فاقد</example>
        [System.Text.Json.Serialization.JsonRequired]
        public Morourak.Domain.Enums.Common.ReplacementType ReplacementType { get; set; }

        /// <summary>
        /// Delivery information and address for the new license.
        /// </summary>
        public DeliveryInfoDto Delivery { get; set; } = null!;
    }
}
