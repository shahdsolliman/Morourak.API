using Morourak.Application.DTOs.Appointments;
using Morourak.Application.Interfaces;
using Morourak.Application.Interfaces.Services;
using Morourak.Domain.Entities;
using Morourak.Domain.Enums.Appointments;
using Morourak.Domain.Enums.Request;
using Morourak.Domain.Extensions;
using AutoMapper;
using System.Globalization;

namespace Morourak.Application.Services
{
    /// <summary>
    /// Handles all appointment queries (read operations and DTO mapping).
    /// </summary>
    public class AppointmentQueryService : IAppointmentQueryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        private static readonly TimeOnly WorkStart = new(9, 0);
        private static readonly TimeOnly WorkEnd = new(14, 0);
        private const int SlotDurationMinutes = 30;

        public AppointmentQueryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AppointmentDto>> GetAvailableSlotsAsync(
            DateOnly date,
            AppointmentType type,
            int trafficUnitId)
        {
            if (date < DateOnly.FromDateTime(DateTime.Today))
                throw new Morourak.Application.Exceptions.ValidationException(
                    "لا يمكن عرض مواعيد لتاريخ سابق.",
                    "INVALID_PAST_DATE");

            var repo = _unitOfWork.Repository<Morourak.Domain.Entities.Appointment>();

            var booked = await repo.FindAsync(a =>
                a.Date == date &&
                a.Type == type &&
                a.TrafficUnitId == trafficUnitId &&
                a.Status != AppointmentStatus.Cancelled);

            var bookedTimes = booked
                .Select(a => a.StartTime)
                .ToHashSet();

            var slots = new List<AppointmentDto>();

            for (var time = WorkStart; time < WorkEnd; time = time.AddMinutes(SlotDurationMinutes))
            {
                if (bookedTimes.Contains(time))
                    continue;

                var appointment = new Morourak.Domain.Entities.Appointment
                {
                    Type = type,
                    Date = date,
                    StartTime = time,
                    Status = AppointmentStatus.Scheduled, // Temporary status for DTO mapping
                    CreatedAt = DateTime.Now,
                    TrafficUnitId = trafficUnitId
                };

                var dto = _mapper.Map<AppointmentDto>(appointment);
                dto.Status = AppointmentStatus.Available.GetDisplayName(); // Set back to Available
                slots.Add(dto);
            }

            return slots;
        }

        public async Task<IEnumerable<AppointmentDto>> GetMyAppointmentsAsync(string nationalId)
        {
            var appointments = await _unitOfWork.Repository<Morourak.Domain.Entities.Appointment>()
                .FindAsync(
                    a => a.CitizenNationalId == nationalId,
                    a => a.Governorate!,
                    a => a.TrafficUnit!);

            return appointments.Select(MapToDto).ToList();
        }

        public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByTypeAsync(AppointmentType type)
        {
            var repo = _unitOfWork.Repository<Morourak.Domain.Entities.Appointment>();
            var appointments = await repo.FindAsync(
                a => a.Type == type && a.Status == AppointmentStatus.Scheduled,
                a => a.Governorate!,
                a => a.TrafficUnit!);

            return appointments
                .OrderByDescending(a => a.Date)
                .ThenByDescending(a => a.StartTime)
                .Select(MapToDto)
                .ToList();
        }

        public async Task<IEnumerable<AppointmentDto>> GetByRoleAsync(string role, string? userId = null, DateOnly? date = null)
        {
            role = role.ToUpperInvariant();

            AppointmentType? type = role switch
            {
                "DOCTOR" => AppointmentType.Medical,
                "EXAMINATOR" => AppointmentType.Driving,
                "INSPECTOR" => AppointmentType.Technical,
                _ => null
            };

            if (type == null)
                return Enumerable.Empty<AppointmentDto>();

            var repo = _unitOfWork.Repository<Morourak.Domain.Entities.Appointment>();
            
            IEnumerable<Morourak.Domain.Entities.Appointment> appointments;

            if (role == "ADMIN")
            {
                appointments = await repo.FindAsync(a =>
                    (!date.HasValue || a.Date == date.Value),
                    a => a.Governorate!,
                    a => a.TrafficUnit!);
            }
            else
            {
                if (type == null) return Enumerable.Empty<AppointmentDto>();

                appointments = await repo.FindAsync(a =>
                    a.Type == type.Value &&
                    a.Status != AppointmentStatus.Cancelled &&
                    (!date.HasValue || a.Date == date.Value) &&
                    (string.IsNullOrEmpty(a.StaffId) || a.StaffId == userId), 
                    a => a.Governorate!,
                    a => a.TrafficUnit!);
            }

            return appointments
                .OrderByDescending(a => a.Date)
                .ThenByDescending(a => a.StartTime)
                .Select(MapToDto)
                .ToList();
        }

        public async Task<AppointmentDto> GetByIdAsync(int id)
        {
            var repo = _unitOfWork.Repository<Morourak.Domain.Entities.Appointment>();
            var appointment = await repo.GetAsync(
                a => a.Id == id,
                a => a.Governorate!,
                a => a.TrafficUnit!);

            if (appointment == null) return null!;

            return MapToDto(appointment);
        }

        private AppointmentDto MapToDto(Morourak.Domain.Entities.Appointment appointment)
        {
            return _mapper.Map<AppointmentDto>(appointment);
        }
    }
}
