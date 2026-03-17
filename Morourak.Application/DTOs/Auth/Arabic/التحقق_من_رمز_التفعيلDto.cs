using System.Text.Json.Serialization;

namespace Morourak.Application.DTOs.Auth.Arabic
{
    /// <summary>
    /// Data required to verify a one-time password (OTP).
    /// </summary>
    public class التحقق_من_رمز_التفعيلDto
    {
        /// <summary>
        /// User's email address.
        /// </summary>
        /// <example>user@example.com</example>
        [JsonPropertyName("البريد الإلكتروني")]
        public string البريد_الإلكتروني { get; set; } = null!;

        /// <summary>
        /// The 6-digit verification code received by email.
        /// </summary>
        /// <example>123456</example>
        [JsonPropertyName("الرمز")]
        public string الرمز { get; set; } = null!;
    }
}
