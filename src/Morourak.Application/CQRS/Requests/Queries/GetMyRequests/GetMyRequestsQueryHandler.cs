using MediatR;
using Morourak.Application.Common;
using Morourak.Application.DTOs;
using Morourak.Application.DTOs.Common;
using Morourak.Application.Interfaces;
using Morourak.Domain.Entities;
using Morourak.Domain.Extensions;
using System.Linq.Expressions;

namespace Morourak.Application.CQRS.Requests.Queries.GetMyRequests;

public sealed class GetMyRequestsQueryHandler
    : IRequestHandler<GetMyRequestsQuery, PagedResult<ServiceRequestSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMyRequestsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<ServiceRequestSummaryDto>> Handle(
        GetMyRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var pagination = request.Pagination ?? new PaginationParams();

        var pagedEntities = await _unitOfWork.Repository<ServiceRequest>()
            .FindPagedAsync(
                predicate: sr => sr.CitizenNationalId == request.NationalId,
                orderBy: q => q.OrderByDescending(sr => sr.SubmittedAt).ThenByDescending(sr => sr.Id),
                pageNumber: pagination.PageNumber,
                pageSize: pagination.PageSize);

        var items = pagedEntities
            .Items
            .Select(sr => new ServiceRequestSummaryDto
            {
                RequestNumber = sr.RequestNumber,
                ServiceType = sr.ServiceType.GetDisplayName(),
                Status = sr.Status.GetDisplayName(),
                SubmittedAt = sr.SubmittedAt
            })
            .ToList();

        return new PagedResult<ServiceRequestSummaryDto>(
            items: items,
            totalCount: pagedEntities.TotalCount,
            pageNumber: pagedEntities.PageNumber,
            pageSize: pagedEntities.PageSize);
    }
}

