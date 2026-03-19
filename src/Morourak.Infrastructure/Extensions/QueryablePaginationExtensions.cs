using Microsoft.EntityFrameworkCore;
using Morourak.Application.Common;
using Morourak.Application.DTOs.Common;

namespace Morourak.Infrastructure.Extensions;

public static class QueryablePaginationExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? PaginationParams.DefaultPageSize : pageSize;
        pageSize = pageSize > PaginationParams.MaxPageSize
            ? PaginationParams.MaxPageSize
            : pageSize;

        var totalCount = await query.CountAsync(cancellationToken);

        if (totalCount == 0)
        {
            return new PagedResult<T>(
                items: Array.Empty<T>(),
                totalCount: 0,
                pageNumber: pageNumber,
                pageSize: pageSize);
        }

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        if (pageNumber > totalPages)
        {
            return new PagedResult<T>(
                items: Array.Empty<T>(),
                totalCount: totalCount,
                pageNumber: pageNumber,
                pageSize: pageSize);
        }

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(
            items: items,
            totalCount: totalCount,
            pageNumber: pageNumber,
            pageSize: pageSize);
    }
}

