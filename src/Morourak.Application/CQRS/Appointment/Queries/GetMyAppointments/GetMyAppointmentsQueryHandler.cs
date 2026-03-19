using MediatR;
using Morourak.Application.DTOs.Appointments;
using Morourak.Application.Common;
using Morourak.Application.DTOs.Common;
using Morourak.Application.Interfaces;

namespace Morourak.Application.CQRS.Appointment.Queries.GetMyAppointments;

public sealed class GetMyAppointmentsQueryHandler
    : IRequestHandler<GetMyAppointmentsQuery, PagedResult<AppointmentSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMyAppointmentsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<AppointmentSummaryDto>> Handle(
        GetMyAppointmentsQuery request,
        CancellationToken cancellationToken)
    {
        var pagination = request.Pagination ?? new PaginationParams();

        var pagedEntities = await _unitOfWork.Repository<Morourak.Domain.Entities.Appointment>()
            .FindPagedAsync(
                predicate: a => a.CitizenNationalId == request.NationalId,
                orderBy: q => q.OrderByDescending(a => a.Date).ThenByDescending(a => a.StartTime),
                pageNumber: pagination.PageNumber,
                pageSize: pagination.PageSize);

        var items = pagedEntities
            .Items
            .Select(a => new AppointmentSummaryDto
            {
                AppointmentId = a.Id,
                ServiceName = a.Type.ToString(),
                Status = a.Status.ToString(),
                Date = a.Date.ToString("yyyy-MM-dd")
            })
            .ToList();

        return new PagedResult<AppointmentSummaryDto>(
            items: items,
            totalCount: pagedEntities.TotalCount,
            pageNumber: pagedEntities.PageNumber,
            pageSize: pagedEntities.PageSize);
    }
}

