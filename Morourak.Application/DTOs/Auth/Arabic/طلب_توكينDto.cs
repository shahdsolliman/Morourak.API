using System.Text.Json.Serialization;

namespace Morourak.Application.DTOs.Auth.Arabic
{
    public class طلب_توكينDto
    {
        [JsonPropertyName("رمز التحديث")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
