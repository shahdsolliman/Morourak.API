using System.Text.Json.Serialization;

namespace Morourak.Application.Common;

public class PagedResult<T>
{
    [JsonPropertyName("العناصر")]
    public List<T> Items { get; set; } = new();

    [JsonPropertyName("الصفحة_الحالية")]
    public int CurrentPage { get; set; }

    [JsonPropertyName("إجمالي_الصفحات")]
    public int TotalPages { get; set; }

    [JsonPropertyName("حجم_الصفحة")]
    public int PageSize { get; set; }

    [JsonPropertyName("إجمالي_العدد")]
    public int TotalCount { get; set; }

    [JsonPropertyName("يوجد_سابق")]
    public bool HasPrevious => CurrentPage > 1;

    [JsonPropertyName("يوجد_تالي")]
    public bool HasNext => CurrentPage < TotalPages;

    public PagedResult() { }

    public PagedResult(List<T> items, int count, int pageNumber, int pageSize)
    {
        TotalCount = count;
        PageSize = pageSize;
        CurrentPage = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        Items = items;
    }
}
