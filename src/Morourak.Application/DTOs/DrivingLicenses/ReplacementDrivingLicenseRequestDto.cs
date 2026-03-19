namespace Morourak.Application.DTOs.DrivingLicenses
{
    public class ReplacementDrivingLicenseRequestDto
    {
        public string ReplacementType { get; set; } = null!; // Lost | Damaged
        public string Governorate { get; set; } = null!;
        public string TrafficUnit { get; set; } = null!;
        public string DeliveryMethod { get; set; } = null!; // Pickup | Mail
        public string? PoliceReportPath { get; set; }
    }
}
