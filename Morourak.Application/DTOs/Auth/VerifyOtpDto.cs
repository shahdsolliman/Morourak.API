/// <summary>
/// Data required to verify a one-time password (OTP).
/// </summary>
public class VerifyOtpDto
{
    /// <summary>
    /// User's email address.
    /// </summary>
    /// <example>user@example.com</example>
    public string Email { get; set; } = null!;

    /// <summary>
    /// The 6-digit verification code received by email.
    /// </summary>
    /// <example>123456</example>
    public string Code { get; set; } = null!;
}
