using Morourak.Application.DTOs.Delivery;

namespace Morourak.API.DTOs.VehicleLicenses
{
    public class IssueReplacementApiDto
    {
        [System.Text.Json.Serialization.JsonRequired]
        public Morourak.Domain.Enums.Common.ReplacementType ReplacementType { get; set; }

        public DeliveryInfoDto Delivery { get; set; } = null!;
    }
}
