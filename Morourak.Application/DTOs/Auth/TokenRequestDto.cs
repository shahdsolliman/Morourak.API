namespace Morourak.Application.DTOs.Auth
{
    /// <summary>
    /// Request DTO for refreshing an expired access token.
    /// </summary>
    public class TokenRequestDto
    {
        /// <summary>
        /// The valid refresh token previously issued to the user.
        /// </summary>
        public string RefreshToken { get; set; } = default!;
    }
}