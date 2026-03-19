using Morourak.Application.DTOs.Appointments;
using Morourak.Domain.Enums.Appointments;

namespace Morourak.Application.Interfaces.Services
{
    public interface IAppointmentService
    {
        // ================= Update Status =================
        Task UpdateStatusAsync(string requestNumber,
            AppointmentType type,
            bool passed,
            string? notes,
            string staffUserId);
    }
}
