using MediatR;
using Morourak.Application.DTOs.Appointments;

namespace Morourak.Application.CQRS.Appointment.Queries.GetMyAppointments;

public sealed record GetMyAppointmentDetailsQuery(int AppointmentId, string NationalId)
    : IRequest<AppointmentDetailsDto?>;

