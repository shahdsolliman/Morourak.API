using System.ComponentModel.DataAnnotations;

namespace Morourak.Application.DTOs.Auth
{
    /// <summary>
    /// Request DTO for requesting a password reset.
    /// </summary>
    public class ForgotPasswordRequestDto
    {
        /// <summary>
        /// The registered email address of the account.
        /// </summary>
        /// <example>user@example.com</example>
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }
}