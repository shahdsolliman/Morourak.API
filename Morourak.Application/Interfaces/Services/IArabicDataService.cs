using Morourak.Application.DTOs.Appointments;

namespace Morourak.Application.Interfaces.Services
{
    public interface IArabicDataService
    {
        Task<ArabicAppointmentDto> GetArabicAppointmentByIdAsync(int appointmentId);
        Task<IEnumerable<ArabicAppointmentDto>> GetArabicAppointmentsByRoleAsync(string role, string? userId = null);
    }
}
