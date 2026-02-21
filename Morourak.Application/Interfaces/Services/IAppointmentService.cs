using Morourak.Application.DTOs.Appointments;
using Morourak.Domain.Enums.Appointments;

namespace Morourak.Application.Interfaces.Services
{
    public interface IAppointmentService
    {
        // ================= Available Slots =================
        Task<IEnumerable<AppointmentDto>> GetAvailableSlotsAsync(DateOnly date, AppointmentType type);

        // ================= Book Appointment =================
        Task<BookedAppointmentDto> BookAsync(
            string nationalId, string requestNumber, int applicationId, AppointmentType type, DateOnly date, TimeOnly startTime);

        // ================= My Appointments =================
        Task<IEnumerable<AppointmentDto>> GetMyAppointmentsAsync(string nationalId);

        // ================= Update Status =================
        Task UpdateStatusAsync(string requestNumber, AppointmentStatus status);

        // ================= Get Appointment By Id =================
        Task<AppointmentDto> GetByIdAsync(int id);

        // ================= Get Appointments By Type =================
        Task<IEnumerable<AppointmentDto>> GetAppointmentsByTypeAsync(AppointmentType type);
    }
}