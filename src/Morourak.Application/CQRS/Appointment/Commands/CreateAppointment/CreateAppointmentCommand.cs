using MediatR;
using Morourak.Application.Common.Interfaces;
using Morourak.Application.DTOs.Appointments;

namespace Morourak.Application.CQRS.Appointment.Commands.CreateAppointment;

public sealed record CreateAppointmentCommand(
    string NationalId,
    string ServiceType,
    DateOnly Date,
    TimeOnly Time,
    int GovernorateId,
    int TrafficUnitId) : IRequest<BookingConfirmationDto>, IInvalidateCacheRequest
{
    public string[] CacheKeysToInvalidate => new[] { $"user:{NationalId}:appointments:*" };
}

