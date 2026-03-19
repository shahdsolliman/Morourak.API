using Morourak.Domain.Entities;
using Morourak.Domain.Enums.Appointments;

namespace Morourak.Application.Interfaces.DomainServices;

public interface IRequestDomainService
{
    Task<ServiceRequest?> FindPrimaryServiceRequestAsync(
        string nationalId,
        AppointmentType appointmentType,
        CancellationToken cancellationToken);
}

