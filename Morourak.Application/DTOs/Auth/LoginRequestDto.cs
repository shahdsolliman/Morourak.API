using System.ComponentModel.DataAnnotations;

namespace Morourak.Application.DTOs.Auth
{
    /// <summary>
    /// Request DTO for user login.
    /// </summary>
    public class LoginRequestDto
    {
        /// <summary>
        /// The registered mobile phone number.
        /// </summary>
        /// <example>01234567890</example>
        [Required]
        public string MobileNumber { get; set; } = null!;

        /// <summary>
        /// The account password.
        /// </summary>
        /// <example>P@ssword123</example>
        [Required]
        public string Password { get; set; } = null!;
    }
}
