namespace Morourak.Application.DTOs.Auth
{
    /// <summary>
    /// Standard response returned after successful authentication operations.
    /// </summary>
    public class AuthResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
