using Morourak.Application.DTOs.Delivery;

namespace Morourak.API.DTOs.DrivingLicenses
{
    public class IssueReplacementDrivingLicenseApiDto
    {
        public string ReplacementType { get; set; } = null!; // "Lost" or "Damaged"
        public DeliveryInfoDto Delivery { get; set; } = null!;
    }
}