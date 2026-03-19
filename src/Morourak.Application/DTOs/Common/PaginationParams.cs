using System.ComponentModel.DataAnnotations;

namespace Morourak.Application.DTOs.Common;

/// <summary>
/// Shared pagination parameters for list endpoints.
/// </summary>
public sealed class PaginationParams
{
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 50;

    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, MaxPageSize)]
    public int PageSize { get; set; } = DefaultPageSize;
}

