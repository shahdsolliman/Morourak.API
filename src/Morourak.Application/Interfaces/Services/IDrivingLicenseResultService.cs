using Morourak.Domain.Enums.Appointments;

namespace Morourak.Application.Interfaces.Services
{
    public interface IDrivingLicenseResultService
    {
        Task SubmitAppointmentResultAsync(int applicationId, AppointmentType type, bool passed, string? notes);
    }
}
