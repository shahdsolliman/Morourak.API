using Morourak.Application.DTOs.Delivery;
using Morourak.Domain.Enums.Common;

namespace Morourak.API.DTOs.VehicleLicenses
{
    /// <summary>
    /// Data required to issue a replacement for a vehicle license.
    /// </summary>
    public class IssueReplacementVehicleLicenseApiDto
    {
        /// <summary>
        /// The reason for replacement.
        /// </summary>
        /// <example>Lost</example>
        [System.Text.Json.Serialization.JsonRequired]
        public ReplacementType ReplacementType { get; set; }

        /// <summary>
        /// Delivery information and address for the new license.
        /// </summary>
        public DeliveryInfoDto Delivery { get; set; } = null!;
    }
}
