using Morourak.Application.Exceptions;

namespace Morourak.Application.DTOs.Auth
{
    /// <summary>
    /// Standard response returned after successful authentication operations.
    /// </summary>
    public class AuthResponseDto
    {
        /// <summary>
        /// Indicates if the operation was successful.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// A descriptive message about the result of the operation.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// The JWT access token (only returned on successful login or refresh).
        /// </summary>
        public string? Token { get; set; }

        /// <summary>
        /// The list of roles assigned to the user.
        /// </summary>
        public List<string>? Roles { get; set; }

        /// <summary>
        /// Specific error code if the operation failed.
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// Detailed validation errors if applicable.
        /// </summary>
        public List<ErrorDetail>? Details { get; set; }

        /// <summary>
        /// The refresh token used to obtain new access tokens.
        /// </summary>
        public string? RefreshToken { get; set; }
    }
}
