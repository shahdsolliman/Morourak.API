using System.ComponentModel.DataAnnotations;

namespace Morourak.Application.DTOs.Auth
{
    /// <summary>
    /// Request DTO used for citizen registration.
    /// Registration is allowed only if NationalId and MobileNumber
    /// match an existing record in CitizenRegistry.
    /// </summary>
    public class RegisterRequestDto
    {
        /// <summary>
        /// The 14-digit national identity number of the citizen.
        /// </summary>
        /// <example>29001011234567</example>
        [Required]
        [StringLength(14, MinimumLength = 14, ErrorMessage = "National ID must be exactly 14 digits.")]
        public string NationalId { get; set; } = null!;

        /// <summary>
        /// The mobile phone number of the citizen.
        /// </summary>
        /// <example>01234567890</example>
        [Required]
        [Phone]
        public string MobileNumber { get; set; } = null!;

        /// <summary>
        /// The chosen username for the account.
        /// </summary>
        /// <example>johndoe</example>
        [Required]
        [MinLength(4)]
        public string Username { get; set; } = null!;

        /// <summary>
        /// The email address for account notifications and verification.
        /// </summary>
        /// <example>john.doe@example.com</example>
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        /// <summary>
        /// The citizen's first name as it appears on their ID.
        /// </summary>
        /// <example>John</example>
        [Required]
        public string FirstName { get; set; } = null!;

        /// <summary>
        /// The citizen's last name as it appears on their ID.
        /// </summary>
        /// <example>Doe</example>
        [Required]
        public string LastName { get; set; } = null!;

        /// <summary>
        /// The password for the new account.
        /// </summary>
        /// <example>P@ssword123</example>
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        /// <summary>
        /// Confirmation of the password to ensure it was entered correctly.
        /// </summary>
        /// <example>P@ssword123</example>
        [Required]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
