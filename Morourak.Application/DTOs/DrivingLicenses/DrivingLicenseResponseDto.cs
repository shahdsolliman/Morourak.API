using Morourak.Application.DTOs.Delivery;
using Morourak.Application.DTOs.Appointments;
using Morourak.Domain.Enums;
using Morourak.Domain.Enums.Driving;

/// <summary>
/// Detailed response containing driving license information after finalization or retrieval.
/// </summary>
public class DrivingLicenseResponseDto
{
    /// <summary>
    /// Internal unique identifier for the license.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Publicly visible driving license number.
    /// </summary>
    public string DrivingLicenseNumber { get; set; }

    /// <summary>
    /// The category of the license.
    /// </summary>
    public string Category { get; set; }

    /// <summary>
    /// Issuing governorate.
    /// </summary>
    public string Governorate { get; set; }

    /// <summary>
    /// Issuing traffic unit.
    /// </summary>
    public string LicensingUnit { get; set; }

    /// <summary>
    /// Date of issuance.
    /// </summary>
    public DateOnly IssueDate { get; set; }

    /// <summary>
    /// Expiration date.
    /// </summary>
    public DateOnly ExpiryDate { get; set; }

    /// <summary>
    /// Current license status.
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// Name of the citizen holding the license.
    /// </summary>
    public string CitizenName { get; set; }

    /// <summary>
    /// Delivery details for the physical license.
    /// </summary>
    public DeliveryInfoDto Delivery { get; set; }
}