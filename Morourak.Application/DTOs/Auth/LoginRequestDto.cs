using System.ComponentModel.DataAnnotations;

namespace Morourak.Application.DTOs.Auth
{
    public class LoginRequestDto
    {
        [Required]
        public string MobileNumber { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }
}
