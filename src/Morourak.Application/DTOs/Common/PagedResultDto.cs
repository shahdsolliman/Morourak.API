using System.Text.Json.Serialization;

namespace Morourak.Application.DTOs.Common;

/// <summary>
/// Standard envelope for paginated results.
/// </summary>
/// <typeparam name="T">The type of data being paginated.</typeparam>
public class PagedResultDto<T>
{
    public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();

    public int TotalCount { get; set; }

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    public bool HasNextPage => PageNumber < TotalPages;

    public bool HasPreviousPage => PageNumber > 1;

    public PagedResultDto(IEnumerable<T> data, int totalCount, int pageNumber, int pageSize)
    {
        Data = data;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
