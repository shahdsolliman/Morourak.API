namespace Morourak.Application.DTOs.DrivingLicenses
{
    public class ReplacementDrivingLicenseRequestDto
    {
        public Morourak.Domain.Enums.Common.ReplacementType ReplacementType { get; set; }
        public string Governorate { get; set; } = null!;
        public string TrafficUnit { get; set; } = null!;
        public Morourak.Domain.Enums.Common.DeliveryMethod DeliveryMethod { get; set; }
        public string? PoliceReportPath { get; set; }
    }
}
