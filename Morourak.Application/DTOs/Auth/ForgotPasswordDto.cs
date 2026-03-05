using System.ComponentModel.DataAnnotations;

namespace Morourak.Application.DTOs.Auth
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }
}
