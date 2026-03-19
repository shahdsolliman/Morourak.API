using MediatR;
using Morourak.Application.Common;
using Morourak.Application.Common.Interfaces;
using Morourak.Application.DTOs;
using Morourak.Application.DTOs.Common;

namespace Morourak.Application.CQRS.Requests.Queries.GetMyRequests;

public sealed record GetMyRequestsQuery(
    string NationalId,
    PaginationParams Pagination)
    : IRequest<PagedResult<ServiceRequestSummaryDto>>, ICacheableRequest
{
    public string CacheKey => $"user:{NationalId}:requests:page:{Pagination.PageNumber}:size:{Pagination.PageSize}";
    
    public TimeSpan? Expiration => null;
}

