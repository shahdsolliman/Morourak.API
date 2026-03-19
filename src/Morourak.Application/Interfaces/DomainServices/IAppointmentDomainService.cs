using Morourak.Application.CQRS.Appointment.Commands.CreateAppointment;
using Morourak.Domain.Enums.Appointments;

namespace Morourak.Application.Interfaces.DomainServices;

public interface IAppointmentDomainService
{
    Task<BookingConfirmationContext> ConfirmBookingAsync(
        string nationalId,
        string serviceType,
        DateOnly date,
        TimeOnly time,
        int governorateId,
        int trafficUnitId,
        CancellationToken cancellationToken);
}

