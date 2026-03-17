using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Morourak.API.DTOs.VehicleLicenses.Arabic
{
    /// <summary>
    /// Request DTO for renewing a vehicle license.
    /// </summary>
    public class طلب_تجديد_رخصة_مركبةApiDto
    {
        /// <summary>
        /// The current vehicle license number to be renewed.
        /// </summary>
        /// <example>V123456</example>
        [Required]
        [JsonPropertyName("رقم رخصة المركبة")]
        public string رقم_رخصة_المركبة { get; set; } = null!;
    }
}
