using System.Text.Json.Serialization;
using Morourak.Application.DTOs.Delivery;

namespace Morourak.Application.DTOs.Vehicles
{
    /// <summary>
    /// Response containing full details of a vehicle license.
    /// </summary>
    public class VehicleLicenseResponseDto
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
        public DateOnly IssueDate { get; set; }

        [JsonPropertyName("تاريخ_الانتهاء")]
        public DateOnly ExpiryDate { get; set; }

        [JsonPropertyName("الرقم_القومي_للمالك")]
        public string CitizenNationalId { get; set; } = null!;

        [JsonPropertyName("اسم_المالك")]
        public string CitizenName { get; set; } = null!;

        [JsonPropertyName("بيانات_التوصيل")]
        public DeliveryInfoDto Delivery { get; set; } = null!;
    }
}
