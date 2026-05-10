using AutoMapper;
using MediatR;
using Morourak.Application.Common;
using Morourak.Application.DTOs;
using Morourak.Application.DTOs.Common;
using Morourak.Application.Interfaces;
using Morourak.Domain.Entities;

namespace Morourak.Application.CQRS.Requests.Queries.GetMyRequests;

public sealed class GetMyRequestsQueryHandler
    : IRequestHandler<GetMyRequestsQuery, PagedResult<ServiceRequestSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetMyRequestsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<ServiceRequestSummaryDto>> Handle(
        GetMyRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var pagination = request.Pagination ?? new PaginationParams();

        var pagedEntities = await _unitOfWork.Repository<ServiceRequest>()
            .FindPagedAsync(
                predicate: sr => sr.CitizenNationalId == request.NationalId,
                orderBy: q => q.OrderByDescending(sr => sr.SubmittedAt).ThenByDescending(sr => sr.RequestNumber),
                pageNumber: pagination.PageNumber,
                pageSize: pagination.PageSize);

        var items = pagedEntities.Items
            .Select(sr => _mapper.Map<ServiceRequestSummaryDto>(sr))
            .ToList();

        return new PagedResult<ServiceRequestSummaryDto>(
            items: items,
            totalCount: pagedEntities.TotalCount,
            pageNumber: pagedEntities.PageNumber,
            pageSize: pagedEntities.PageSize);
    }
}

