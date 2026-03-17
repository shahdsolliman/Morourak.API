using System.Text.Json.Serialization;
using Morourak.Application.DTOs.Delivery;
using Morourak.Application.DTOs.Appointments;
using Morourak.Domain.Enums;
using Morourak.Domain.Enums.Driving;

/// <summary>
/// Detailed response containing driving license information after finalization or retrieval.
/// </summary>
public class DrivingLicenseResponseDto
{
    [JsonPropertyName("المعرف")]
    public int Id { get; set; }

    [JsonPropertyName("رقم_الرخصة_القيادة")]
    public string DrivingLicenseNumber { get; set; }

    [JsonPropertyName("الفئة")]
    public string Category { get; set; }

    [JsonPropertyName("المحافظة")]
    public string Governorate { get; set; }

    [JsonPropertyName("وحدة_الترخيص")]
    public string LicensingUnit { get; set; }

    [JsonPropertyName("تاريخ_الإصدار")]
    public DateOnly IssueDate { get; set; }

    [JsonPropertyName("تاريخ_الانتهاء")]
    public DateOnly ExpiryDate { get; set; }

    [JsonPropertyName("الحالة")]
    public string Status { get; set; }

    [JsonPropertyName("اسم_المواطن")]
    public string CitizenName { get; set; }

    [JsonPropertyName("بيانات_التوصيل")]
    public DeliveryInfoDto Delivery { get; set; }
}