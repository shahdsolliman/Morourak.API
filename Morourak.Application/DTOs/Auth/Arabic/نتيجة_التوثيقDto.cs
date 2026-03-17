using System.Text.Json.Serialization;
using Morourak.Application.Exceptions;

namespace Morourak.Application.DTOs.Auth.Arabic
{
    /// <summary>
    /// Standard response returned after successful authentication operations.
    /// </summary>
    public class نتيجة_التوثيقDto
    {
        /// <summary>
        /// Indicates if the operation was successful.
        /// </summary>
        [JsonPropertyName("ناجح")]
        public bool ناجح { get; set; }

        /// <summary>
        /// A descriptive message about the result of the operation.
        /// </summary>
        [JsonPropertyName("الرسالة")]
        public string الرسالة { get; set; } = string.Empty;

        /// <summary>
        /// The JWT access token (only returned on successful login or refresh).
        /// </summary>
        [JsonPropertyName("التوكين")]
        public string? التوكين { get; set; }

        /// <summary>
        /// The list of roles assigned to the user.
        /// </summary>
        [JsonPropertyName("الأدوار")]
        public List<string>? الأدوار { get; set; }

        /// <summary>
        /// Specific error code if the operation failed.
        /// </summary>
        [JsonPropertyName("رمز الخطأ")]
        public string? رمز_الخطأ { get; set; }

        /// <summary>
        /// Detailed validation errors if applicable.
        /// </summary>
        [JsonPropertyName("التفاصيل")]
        public List<ErrorDetail>? التفاصيل { get; set; }

        /// <summary>
        /// The refresh token used to obtain new access tokens.
        /// </summary>
        [JsonPropertyName("رمز التحديث")]
        public string? رمز_التحديث { get; set; }
    }
}
