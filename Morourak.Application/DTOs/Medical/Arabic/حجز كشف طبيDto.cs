using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Morourak.Application.DTOs.Medical.Arabic
{
    public class حجز_كشف_طبيDto
    {
        [Required]
        [JsonPropertyName("رقم الطلب")]
        public int رقم_الطلب { get; set; }

        [Required]
        [JsonPropertyName("الرقم القومي")]
        public string الرقم_القومي { get; set; } = null!;

        [Required]
        [JsonPropertyName("تاريخ الموعد")]
        public DateTime تاريخ_الموعد { get; set; }
    }
}
