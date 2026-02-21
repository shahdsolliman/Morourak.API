using System.ComponentModel.DataAnnotations;

public class ForgotPasswordRequestDto
{
    [Required] public string Email { get; set; } = null!;
}