using System.Text.Json.Serialization;

namespace Morourak.API.Common;

/// <summary>
/// Unified API response envelope with Arabic JSON keys.
/// Internal C# properties remain in English; Arabic names appear only in serialized JSON.
/// </summary>
public class ApiResponseArabic
{
    [JsonPropertyName("isSuccess")]
    public bool IsSuccess { get; set; }

    [JsonPropertyName("رسالة")]
    public string? Message { get; set; }

    [JsonPropertyName("رمز_الخطأ")]
    public string? ErrorCode { get; set; }

    [JsonPropertyName("التفاصيل")]
    public object? Details { get; set; }

    // ───────────────── Factory Methods ─────────────────

    /// <summary>Creates a success response with optional data and message.</summary>
    public static ApiResponseArabic Success(object? data = null, string? message = null)
    {
        return new ApiResponseArabic
        {
            IsSuccess = true,
            Message = message ?? "تمت العملية بنجاح",
            Details = data
        };
    }

    /// <summary>Creates a failure response.</summary>
    public static ApiResponseArabic Fail(string message, string? errorCode = null, object? details = null)
    {
        return new ApiResponseArabic
        {
            IsSuccess = false,
            Message = message,
            ErrorCode = errorCode,
            Details = details
        };
    }

    /// <summary>Creates a validation-error response from model-state errors.</summary>
    public static ApiResponseArabic ValidationFail(object? details = null, string? traceId = null)
    {
        return new ApiResponseArabic
        {
            IsSuccess = false,
            Message = "يوجد خطأ في البيانات المدخلة.",
            ErrorCode = "VALIDATION_ERROR",
            Details = details
        };
    }
}
