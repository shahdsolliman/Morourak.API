using MediatR;
using Morourak.Application.Common;
using Morourak.Application.Common.Interfaces;
using Morourak.Application.DTOs.Appointments;
using Morourak.Application.DTOs.Common;

namespace Morourak.Application.CQRS.Appointment.Queries.GetMyAppointments;

public sealed record GetMyAppointmentsQuery(
    string NationalId,
    PaginationParams Pagination)
    : IRequest<PagedResult<AppointmentSummaryDto>>, ICacheableRequest
{
    public string CacheKey => $"user:{NationalId}:appointments:page:{Pagination.PageNumber}:size:{Pagination.PageSize}";
    
    public TimeSpan? Expiration => null; // Use default from settings
}

