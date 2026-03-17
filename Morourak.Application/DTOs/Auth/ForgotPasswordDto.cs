using System.ComponentModel.DataAnnotations;

namespace Morourak.Application.DTOs.Auth
{
    /// <summary>
    /// Data required to initiate a password reset.
    /// </summary>
    public class ForgotPasswordDto
    {
        /// <summary>
        /// The user's registered email address.
        /// </summary>
        /// <example>user@example.com</example>
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }
}
