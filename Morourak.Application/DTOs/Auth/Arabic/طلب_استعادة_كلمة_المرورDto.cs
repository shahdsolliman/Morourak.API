using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Morourak.Application.DTOs.Auth.Arabic
{
    /// <summary>
    /// Request DTO for requesting a password reset.
    /// </summary>
    public class طلب_استعادة_كلمة_المرورDto
    {
        /// <summary>
        /// The registered email address of the account.
        /// </summary>
        /// <example>user@example.com</example>
        [Required]
        [EmailAddress]
        [JsonPropertyName("البريد الإلكتروني")]
        public string البريد_الإلكتروني { get; set; } = null!;
    }
}
