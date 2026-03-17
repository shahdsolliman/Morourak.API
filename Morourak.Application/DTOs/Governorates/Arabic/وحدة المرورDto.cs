using System.Text.Json.Serialization;

namespace Morourak.Application.DTOs.Governorates.Arabic
{
    /// <summary>
    /// Data transfer object for traffic unit information.
    /// </summary>
    public class وحدة_المرورDto
    {
        /// <summary>Unique identifier for the traffic unit.</summary>
        [JsonPropertyName("Id")]
        public int Id { get; set; }

        /// <summary>Name of the traffic unit.</summary>
        [JsonPropertyName("الاسم")]
        public string الاسم { get; set; } = null!;

        /// <summary>Physical address of the traffic unit.</summary>
        [JsonPropertyName("العنوان")]
        public string? العنوان { get; set; }

        /// <summary>Working hours of the traffic unit.</summary>
        [JsonPropertyName("مواعيد العمل")]
        public string? مواعيد_العمل { get; set; }
    }
}
