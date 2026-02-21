using Morourak.Application.DTOs.Delivery;
using Morourak.Application.DTOs.Appointments;
using Morourak.Domain.Enums;
using Morourak.Domain.Enums.Driving;

public class DrivingLicenseResponseDto
{
    public int Id { get; set; }
    public string DrivingLicenseNumber { get; set; }
    public string Category { get; set; }
    public string Governorate { get; set; }
    public string LicensingUnit { get; set; }

    public DateOnly IssueDate { get; set; }
    public DateOnly ExpiryDate { get; set; }
    public string Status { get; set; }

    public string CitizenName { get; set; }
    public DeliveryInfoDto Delivery { get; set; }


}