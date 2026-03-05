using Morourak.Application.Exceptions;

namespace Morourak.Application.DTOs.Auth
{
    /// <summary>
    /// Standard response returned after successful authentication operations.
    /// </summary>
    public class AuthResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public List<string>? Roles { get; set; }
        public string? ErrorCode { get; set; }

        public List<ErrorDetail>? Details { get; set; }
        public string? RefreshToken { get; set; }

    }

}
