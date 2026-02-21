using System.ComponentModel.DataAnnotations;

namespace Morourak.Application.DTOs.Auth
{
    public class LoginRequestDto
    {
        [Required]
        public string PhoneNumber { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }
}
