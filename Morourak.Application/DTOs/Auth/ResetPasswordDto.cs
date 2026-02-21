
using System.ComponentModel.DataAnnotations;

public class ResetPasswordDto
{
    [Required] public string Email { get; set; } = null!;
    [Required] public string Code { get; set; } = null!;
    [Required] public string NewPassword { get; set; } = null!;
    [Required] public string ConfirmPassword { get; set; } = null!;
}
