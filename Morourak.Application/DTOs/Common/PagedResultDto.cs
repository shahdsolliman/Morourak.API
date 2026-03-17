using System.Text.Json.Serialization;

namespace Morourak.Application.DTOs.Common;

/// <summary>
/// Standard envelope for paginated results.
/// </summary>
/// <typeparam name="T">The type of data being paginated.</typeparam>
public class PagedResultDto<T>
{
    [JsonPropertyName("البيانات")]
    public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();

    [JsonPropertyName("إجمالي_العناصر")]
    public int TotalCount { get; set; }

    [JsonPropertyName("رقم_الصفحة")]
    public int PageNumber { get; set; }

    [JsonPropertyName("حجم_الصفحة")]
    public int PageSize { get; set; }

    [JsonPropertyName("إجمالي_الصفحات")]
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    [JsonPropertyName("يوجد_صفحة_تالية")]
    public bool HasNextPage => PageNumber < TotalPages;

    [JsonPropertyName("يوجد_صفحة_سابقة")]
    public bool HasPreviousPage => PageNumber > 1;

    public PagedResultDto(IEnumerable<T> data, int totalCount, int pageNumber, int pageSize)
    {
        Data = data;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
