using System.Text.Json.Serialization;

namespace Morourak.Application.DTOs.Governorates.Arabic
{
    /// <summary>
    /// Data transfer object for governorate information.
    /// </summary>
    public class المحافظةDto
    {
        /// <summary>Unique identifier for the governorate.</summary>
        [JsonPropertyName("Id")]
        public int Id { get; set; }

        /// <summary>Name of the governorate.</summary>
        [JsonPropertyName("الاسم")]
        public string الاسم { get; set; } = null!;
    }
}
