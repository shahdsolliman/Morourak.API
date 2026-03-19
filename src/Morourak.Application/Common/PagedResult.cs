namespace Morourak.Application.Common;

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = Array.Empty<T>();

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages { get; set; }

    public bool HasNextPage => PageNumber < TotalPages;

    public bool HasPreviousPage => PageNumber > 1;

    public PagedResult() { }

    public PagedResult(
        IEnumerable<T> items,
        int totalCount,
        int pageNumber,
        int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = pageSize <= 0
            ? 0
            : (int)Math.Ceiling(totalCount / (double)pageSize);
    }
}
