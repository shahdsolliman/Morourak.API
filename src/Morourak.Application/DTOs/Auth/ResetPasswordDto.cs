using System.ComponentModel.DataAnnotations;

namespace Morourak.Application.DTOs.Auth
{
    /// <summary>
    /// Data required to reset a user's password using an OTP.
    /// </summary>
    public class ResetPasswordDto
    {
        /// <summary>
        /// User's email address.
        /// </summary>
        /// <example>user@example.com</example>
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        /// <summary>
        /// The verification code (OTP) received by email.
        /// </summary>
        /// <example>123456</example>
        [Required]
        public string Code { get; set; } = null!;

        /// <summary>
        /// The new password for the account.
        /// </summary>
        /// <example>NewP@ssword123</example>
        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = null!;
    }
}
