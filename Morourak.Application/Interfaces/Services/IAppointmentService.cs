using Morourak.Application.DTOs.Appointments;
using Morourak.Domain.Enums.Appointments;

namespace Morourak.Application.Interfaces.Services
{
    public interface IAppointmentService
    {
        // ================= Available Slots =================
        Task<IEnumerable<AppointmentDto>> GetAvailableSlotsAsync(DateOnly date, AppointmentType type, int trafficUnitId);

        // ================= Confirm Booking (atomic) =================
        Task<BookingConfirmationDto> ConfirmBookingAsync(string nationalId, ConfirmAppointmentRequestDto request);

        // ================= My Appointments =================
        Task<IEnumerable<AppointmentDto>> GetMyAppointmentsAsync(string nationalId);

        // ================= Update Status =================
        Task UpdateStatusAsync(int applicationId,
            AppointmentType type,
            bool passed,
            string? notes,
            string staffUserId);

        // ================= Get Appointment By Id =================
        Task<AppointmentDto> GetByIdAsync(int id);

        // ================= Get Appointments By Type =================
        Task<IEnumerable<AppointmentDto>> GetAppointmentsByTypeAsync(AppointmentType type);

        // ================= Get Appointments By Role =================
        Task<IEnumerable<AppointmentDto>> GetByRoleAsync(string role, string? userId = null);
    }
}