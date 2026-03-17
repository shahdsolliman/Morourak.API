using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Morourak.Application.DTOs.Auth.Arabic
{
    /// <summary>
    /// Data required to reset a user's password using an OTP.
    /// </summary>
    public class إعادة_تعيين_كلمة_المرورDto
    {
        /// <summary>
        /// User's email address.
        /// </summary>
        /// <example>user@example.com</example>
        [Required]
        [EmailAddress]
        [JsonPropertyName("البريد الإلكتروني")]
        public string البريد_الإلكتروني { get; set; } = null!;

        /// <summary>
        /// The verification code (OTP) received by email.
        /// </summary>
        /// <example>123456</example>
        [Required]
        [JsonPropertyName("الرمز")]
        public string الرمز { get; set; } = null!;

        /// <summary>
        /// The new password for the account.
        /// </summary>
        /// <example>NewP@ssword123</example>
        [Required]
        [MinLength(6)]
        [JsonPropertyName("كلمة المرور الجديدة")]
        public string كلمة_المرور_الجديدة { get; set; } = null!;
    }
}
