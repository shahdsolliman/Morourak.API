using Morourak.Domain.Enums.Appointments;

namespace Morourak.Application.Interfaces.Services
{
    public interface IApplicationValidationService
    {
        Task<(bool IsValid, string Message, int ApplicationId)> ValidateApplicationAsync(
            string nationalId,
            string requestNumber,
            AppointmentType type);
    }
}