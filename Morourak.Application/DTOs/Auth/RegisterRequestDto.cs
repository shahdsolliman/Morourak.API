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
        [Required]
        [StringLength(14, MinimumLength = 14, ErrorMessage = "National ID must be exactly 14 digits.")]
        public string NationalId { get; set; } = null!;

        [Required]
        [Phone]
        public string MobileNumber { get; set; } = null!;

        [Required]
        [MinLength(4)]
        public string Username { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string FirstName { get; set; } = null!;

        [Required]
        public string LastName { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
