using Morourak.Application.DTOs.Delivery;

namespace Morourak.API.DTOs.VehicleLicenses
{
    public class IssueReplacementApiDto
    {
        public string ReplacementType { get; set; } 
        public DeliveryInfoDto Delivery { get; set; } 
    }
}