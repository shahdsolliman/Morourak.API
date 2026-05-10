using MediatR;
using Morourak.Application.Common.Interfaces;
using Morourak.Application.DTOs.Appointments;
using Morourak.Domain.Enums.Appointments;

namespace Morourak.Application.CQRS.Appointment.Commands.CreateAppointment;

public sealed record CreateAppointmentCommand(
    string NationalId,
    AppointmentType AppointmentType,
    DateOnly Date,
    TimeOnly Time,
    int GovernorateId,
    int TrafficUnitId,
    string? RequestNumber = null) : IRequest<BookingConfirmationDto>, IInvalidateCacheRequest
{
    public string[] CacheKeysToInvalidate => new[] { $"user:{NationalId}:appointments:*" };
}

