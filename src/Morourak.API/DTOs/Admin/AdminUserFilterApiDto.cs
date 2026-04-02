using Microsoft.AspNetCore.Mvc;
using Morourak.API.Extensions.ModelBinders;
using Morourak.Application.DTOs.Admin;
using Morourak.Application.Enums.Admin;

namespace Morourak.API.DTOs.Admin;

/// <summary>
/// API request model for admin user filtering.
/// Keeps backward compatibility for query string inputs (e.g., unknown SortBy values).
/// </summary>
public sealed class AdminUserFilterApiDto
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }

    [ModelBinder(BinderType = typeof(TolerantDisplayNameNullableEnumModelBinder))]
    public UserSortField? SortBy { get; set; }

    public bool IsDescending { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public UserFilterDto ToApplicationDto()
    {
        return new UserFilterDto
        {
            Search = Search,
            IsActive = IsActive,
            SortBy = SortBy,
            IsDescending = IsDescending,
            PageNumber = PageNumber,
            PageSize = PageSize
        };
    }
}
