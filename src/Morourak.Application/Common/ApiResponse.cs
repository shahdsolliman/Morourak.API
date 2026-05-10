using System.Text.Json.Serialization;

namespace Morourak.Application.Common;

public class ApiResponse<T>
{
    [JsonPropertyName("isSuccess")]
    public bool IsSuccess { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("errorCode")]
    public string? ErrorCode { get; set; }

    [JsonPropertyName("details")]
    public T? Details { get; set; }

    [JsonPropertyName("errors")]
    public List<string>? Errors { get; set; }

    public static ApiResponse<T> SuccessResult(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            IsSuccess = true,
            Message = message,
            Details = data
        };
    }

    public static ApiResponse<T> FailureResult(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            IsSuccess = false,
            Message = message,
            ErrorCode = null,
            Errors = errors
        };
    }

    public static ApiResponse<T> FailureResult(string message, string errorCode, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            IsSuccess = false,
            Message = message,
            ErrorCode = errorCode,
            Errors = errors
        };
    }
}

public class PagedApiResponse<T> : ApiResponse<IEnumerable<T>>
{
    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }

    [JsonPropertyName("totalRecords")]
    public int TotalRecords { get; set; }

    [JsonConstructor]
    public PagedApiResponse()
    {
        Details = Enumerable.Empty<T>();
    }

    public PagedApiResponse(IEnumerable<T> data, int pageNumber, int pageSize, int totalRecords, string? message = null, bool isSuccess = true)
    {
        Details = data;
        Page = pageNumber;
        PageSize = pageSize;
        TotalRecords = totalRecords;
        TotalPages = pageSize > 0 ? (int)Math.Ceiling(totalRecords / (double)pageSize) : 0;
        IsSuccess = isSuccess;
        Message = message;
    }
}
