using Morourak.Application.DTOs.Appointments;
using Morourak.Application.Interfaces;
using Morourak.Application.Interfaces.Services;
using Morourak.Domain.Entities;
using Morourak.Domain.Enums.Appointments;
using Morourak.Domain.Enums.Request;

namespace Morourak.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRequestNumberGenerator _generator;

        public AppointmentService(IUnitOfWork unitOfWork, IRequestNumberGenerator generator)
        {
            _unitOfWork = unitOfWork;
            _generator = generator;
        }

        // ================= Available Slots =================
        public async Task<IEnumerable<AppointmentDto>> GetAvailableSlotsAsync(DateOnly date, AppointmentType type)
        {
            var slots = new List<AppointmentDto>();
            for (int h = 9; h <= 16; h++)
            {
                slots.Add(new AppointmentDto
                {
                    Type = type,
                    Date = date,
                    StartTime = new TimeOnly(h, 0),
                    EndTime = new TimeOnly(h, 30),
                    Status = AppointmentStatus.Available,
                    CreatedAt = DateTime.UtcNow
                });
            }
            return slots;
        }

        // ================= Book Appointment =================
        public async Task<BookedAppointmentDto> BookAsync(string nationalId, string requestNumber, int applicationId, AppointmentType type, DateOnly date, TimeOnly startTime)
        {
            var serviceRequest = (await _unitOfWork.Repository<ServiceRequest>()
                .FindAsync(sr => sr.RequestNumber == requestNumber &&
                                 sr.CitizenNationalId == nationalId))
                .FirstOrDefault();

            if (serviceRequest == null)
                throw new KeyNotFoundException("Service request not found.");

            var repo = _unitOfWork.Repository<Appointment>();

            var slotTaken = await repo.FindAsync(a =>
                a.Type == type &&
                a.Date == date &&
                a.StartTime == startTime &&
                a.Status != AppointmentStatus.Cancelled);

            if (slotTaken.Any())
                throw new InvalidOperationException("This time slot is no longer available.");

            var appointment = new Appointment
            {
                CitizenNationalId = nationalId,
                ApplicationId = applicationId,
                Date = date,
                StartTime = startTime,
                EndTime = startTime.AddMinutes(30),
                Status = AppointmentStatus.Scheduled,
                Type = type,
                RequestNumber = requestNumber
            };

            await repo.AddAsync(appointment);
            await _unitOfWork.CommitAsync();

            return new BookedAppointmentDto
            {
                ServiceNumber = requestNumber,
                ApplicationId = applicationId,
                Date = date,
                StartTime = startTime,
                Status = AppointmentStatus.Scheduled,
                NationalId = nationalId,
                Type = type
            };
        }

        // ================= My Appointments =================
        public async Task<IEnumerable<AppointmentDto>> GetMyAppointmentsAsync(string nationalId)
        {
            var appointments = await _unitOfWork.Repository<Appointment>()
                .FindAsync(a => a.CitizenNationalId == nationalId);

            return appointments.Select(MapToDto).ToList();
        }

        // ================= Update Status =================
        public async Task UpdateStatusAsync(string requestNumber, AppointmentStatus status)
        {
            var repo = _unitOfWork.Repository<Appointment>();

            var appointment = (await repo.FindAsync(a => a.RequestNumber == requestNumber))
                                .OrderByDescending(a => a.CreatedAt)
                                .FirstOrDefault();

            if (appointment == null)
                throw new InvalidOperationException("Appointment not found for the given RequestNumber");

            appointment.Status = status;
            repo.Update(appointment);

            // تحديث الـ ServiceRequest المرتبط
            var serviceRequestRepo = _unitOfWork.Repository<ServiceRequest>();
            var serviceRequest = (await serviceRequestRepo.FindAsync(
                sr => sr.RequestNumber == requestNumber)).FirstOrDefault();

            if (serviceRequest != null)
            {
                serviceRequest.Status = (status == AppointmentStatus.Passed)
                                            ? RequestStatus.Passed
                                            : RequestStatus.Pending;
                serviceRequest.LastUpdatedAt = DateTime.UtcNow;
                serviceRequestRepo.Update(serviceRequest);
            }

            await _unitOfWork.CommitAsync();
        }

        // ================= Mapping Helper =================
        private AppointmentDto MapToDto(Appointment appointment)
        {
            return new AppointmentDto
            {
                RequestNumber = appointment.RequestNumber,
                ApplicationId = appointment.ApplicationId,
                Type = appointment.Type,
                Date = appointment.Date,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                Status = appointment.Status,
                CreatedAt = appointment.CreatedAt,
                CompletedAt = appointment.UpdatedAt,
                CitizenNationalId = appointment.CitizenNationalId
            };
        }

        // ================= Get Appointments By Type =================
        public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByTypeAsync(AppointmentType type)
        {
            var repo = _unitOfWork.Repository<Appointment>();
            var allAppointments = await repo.GetAllAsync();

            return allAppointments.Where(a => a.Type == type)
                                  .Select(MapToDto)
                                  .ToList();
        }

        public async Task<AppointmentDto> GetByIdAsync(int id)
        {
            var repo = _unitOfWork.Repository<Appointment>();
            var appointment = await repo.GetAsync(a => a.Id == id);
            if (appointment == null) return null;

            return MapToDto(appointment);
        }
    }
}