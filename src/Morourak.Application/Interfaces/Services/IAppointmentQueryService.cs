using Morourak.Application.DTOs.Appointments;
using Morourak.Domain.Enums.Appointments;

namespace Morourak.Application.Interfaces.Services
{
    /// <summary>
    /// Read‑only operations for querying appointments.
    /// Separated from command workflows to better respect SRP.
    /// </summary>
    public interface IAppointmentQueryService
    {
        Task<IEnumerable<AppointmentDto>> GetAvailableSlotsAsync(DateOnly date, AppointmentType type, int trafficUnitId);
        Task<IEnumerable<AppointmentDto>> GetMyAppointmentsAsync(string nationalId);
        Task<IEnumerable<AppointmentDto>> GetAppointmentsByTypeAsync(AppointmentType type);
        Task<IEnumerable<AppointmentDto>> GetByRoleAsync(string role, string? userId = null);
        Task<AppointmentDto> GetByIdAsync(int id);
    }
}

