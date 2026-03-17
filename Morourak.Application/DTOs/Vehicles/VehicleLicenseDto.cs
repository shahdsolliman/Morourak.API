using System;
using System.Text.Json.Serialization;

namespace Morourak.Application.DTOs.Vehicles
{
    /// <summary>
    /// Represents a vehicle license issued to a citizen.
    /// </summary>
    public class VehicleLicenseDto
    {
        [JsonPropertyName("المعرف")]
        public int Id { get; set; }

        [JsonPropertyName("رقم_رخصة_المركبة")]
        public string VehicleLicenseNumber { get; set; } = null!;

        [JsonPropertyName("رقم_اللوحة")]
        public string PlateNumber { get; set; } = null!;

        [JsonPropertyName("نوع_المركبة")]
        public string VehicleType { get; set; } = null!;

        [JsonPropertyName("الماركة")]
        public string Brand { get; set; } = null!;

        [JsonPropertyName("الموديل")]
        public string Model { get; set; } = null!;

        [JsonPropertyName("الحالة")]
        public string Status { get; set; } = null!;

        [JsonPropertyName("تاريخ_الإصدار")]
        public DateTime IssueDate { get; set; }

        [JsonPropertyName("تاريخ_الانتهاء")]
        public DateTime ExpiryDate { get; set; }

        [JsonPropertyName("الرقم_القومي_للمالك")]
        public string CitizenNationalId { get; set; } = null!;
    }
}
