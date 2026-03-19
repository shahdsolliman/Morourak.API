namespace Morourak.Application.DTOs.Auth
{
    /// <summary>
    /// Response DTO after a login attempt.
    /// </summary>
    public class LoginResponseDto
    {
        /// <summary>
        /// Indicates if the login was successful.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// A message describing the result of the login.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// The JWT access token (only if successful).
        /// </summary>
        public string? Token { get; set; }

        /// <summary>
        /// The expiration date and time of the access token.
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
    }
}
