using System.Text.Json.Serialization;

namespace Morourak.Application.DTOs.Delivery.Arabic
{
    public class عنوانDto
    {
        [JsonPropertyName("المحافظة")]
        public string المحافظة { get; set; } = default!;

        [JsonPropertyName("المدينة")]
        public string المدينة { get; set; } = default!;

        [JsonPropertyName("التفاصيل")]
        public string التفاصيل { get; set; } = default!;
    }
}
