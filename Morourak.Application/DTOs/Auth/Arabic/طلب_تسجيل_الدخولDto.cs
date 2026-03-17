using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Morourak.Application.DTOs.Auth.Arabic
{
    /// <summary>
    /// Request DTO for user login.
    /// </summary>
    public class طلب_تسجيل_الدخولDto
    {
        /// <summary>
        /// The registered mobile phone number.
        /// </summary>
        /// <example>01234567890</example>
        [Required]
        [JsonPropertyName("رقم الموبايل")]
        public string رقم_الموبايل { get; set; } = null!;

        /// <summary>
        /// The account password.
        /// </summary>
        /// <example>P@ssword123</example>
        [Required]
        [JsonPropertyName("كلمة المرور")]
        public string كلمة_المرور { get; set; } = null!;
    }
}
