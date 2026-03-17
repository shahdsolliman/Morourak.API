using Morourak.Domain.Enums.Appointments;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Morourak.Application.DTOs.Medical.Arabic
{
    public class تسجيل_نتيجة_الكشفDto
    {
        [Required]
        [JsonPropertyName("رقم الكشف")]
        public int رقم_الكشف { get; set; }

        [Required]
        [JsonPropertyName("الحالة")]
        public AppointmentStatus الحالة { get; set; } // Passed or Failed

        [JsonPropertyName("ملاحظات الطبيب")]
        public string? ملاحظات_الطبيب { get; set; }
    }
}
