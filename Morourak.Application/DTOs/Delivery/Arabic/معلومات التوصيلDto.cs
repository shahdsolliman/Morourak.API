using System.Text.Json.Serialization;
using Morourak.Domain.Enums.Common;

namespace Morourak.Application.DTOs.Delivery.Arabic
{
    public class معلومات_التوصيلDto
    {
        [JsonPropertyName("طريقة التوصيل")]
        public DeliveryMethod طريقة_التوصيل { get; set; }

        [JsonPropertyName("العنوان")]
        public عنوانDto? العنوان { get; set; }
    }
}
