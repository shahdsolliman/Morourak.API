using Morourak.Application.DTOs.Appointments;
using Morourak.Application.Interfaces;
using Morourak.Application.Interfaces.Services;
using Morourak.Domain.Entities;
using Morourak.Domain.Enums.Appointments;
using Morourak.Domain.Enums.Request;
using Morourak.Domain.Extensions;
using System.Globalization;

namespace Morourak.Application.Services
{
    /// <summary>
    /// Handles all appointment queries (read operations and DTO mapping).
    /// </summary>
    public class AppointmentQueryService : IAppointmentQueryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AppointmentQueryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<AppointmentDto>> GetMyAppointmentsAsync(string nationalId)
        {
            var appointments = await _unitOfWork.Repository<Appointment>()
                .FindAsync(
                    a => a.CitizenNationalId == nationalId,
                    a => a.Governorate!,
                    a => a.TrafficUnit!);

            return appointments.Select(MapToDto).ToList();
        }

        public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByTypeAsync(AppointmentType type)
        {
            var repo = _unitOfWork.Repository<Appointment>();
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

        public async Task<IEnumerable<AppointmentDto>> GetByRoleAsync(string role, string? userId = null)
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

            var repo = _unitOfWork.Repository<Appointment>();
            var appointments = await repo.FindAsync(a =>
                a.Type == type.Value &&
                a.Status != AppointmentStatus.Cancelled,
                a => a.Governorate!,
                a => a.TrafficUnit!);

            IEnumerable<Appointment> query = appointments;

            return query
                .OrderByDescending(a => a.Date)
                .ThenByDescending(a => a.StartTime)
                .Select(MapToDto)
                .ToList();
        }

        public async Task<AppointmentDto> GetByIdAsync(int id)
        {
            var repo = _unitOfWork.Repository<Appointment>();
            var appointment = await repo.GetAsync(
                a => a.Id == id,
                a => a.Governorate!,
                a => a.TrafficUnit!);

            if (appointment == null) return null!;

            return MapToDto(appointment);
        }

        private static AppointmentDto MapToDto(Appointment appointment)
        {
            var typeName = GetAppointmentTypeName(appointment.Type);
            var assignedToUserId = ResolveAssignedToUserId(appointment.Type);

            var governorateName = string.IsNullOrWhiteSpace(appointment.Governorate?.Name)
                ? "غير محدد"
                : appointment.Governorate!.Name;

            var trafficUnitName = string.IsNullOrWhiteSpace(appointment.TrafficUnit?.Name)
                ? "غير محدد"
                : appointment.TrafficUnit!.Name;

            return new AppointmentDto
            {
                RequestNumberRelated = appointment.RequestNumber,
                ApplicationId = appointment.ApplicationId,
                Type = appointment.Type,
                TypeName = typeName,
                ServiceName = typeName,
                Date = appointment.Date,
                DateFormatted = FormatArabicDate(appointment.Date),
                StartTime = appointment.StartTime,
                TimeFormatted = FormatArabicTime(appointment.StartTime),
                EndTime = appointment.EndTime,
                Status = appointment.Status,
                CreatedAt = FormatArabicDateTime(appointment.CreatedAt),
                CompletedAt = appointment.UpdatedAt.HasValue ? FormatArabicDateTime(appointment.UpdatedAt.Value) : "غير مكتمل",
                CitizenNationalId = appointment.CitizenNationalId,
                GovernorateId = appointment.GovernorateId,
                TrafficUnitId = appointment.TrafficUnitId,
                GovernorateName = governorateName,
                TrafficUnitName = trafficUnitName,
                AssignedToUserId = assignedToUserId
            };
        }

        private static string GetAppointmentTypeName(AppointmentType type)
        {
            return type switch
            {
                AppointmentType.Medical => "كشف طبي",
                AppointmentType.Driving => "اختبار قيادة",
                AppointmentType.Technical => "فحص فني",
                _ => "غير محدد"
            };
        }

        private static string ResolveAssignedToUserId(AppointmentType appointmentType)
        {
            return appointmentType switch
            {
                AppointmentType.Medical => "DOCTOR",
                AppointmentType.Technical => "INSPECTOR",
                AppointmentType.Driving => "EXAMINATOR",
                _ => "STAFF"
            };
        }

        private static string FormatArabicDate(DateOnly date)
        {
            return date.ToString("d MMMM yyyy", new CultureInfo("ar-EG"));
        }

        private static string FormatArabicTime(TimeOnly time)
        {
            return time
                .ToString("hh:mm tt", new CultureInfo("en-US"))
                .Replace("AM", "صباحاً")
                .Replace("PM", "مساءً");
        }

        private static string FormatArabicDateTime(DateTime dateTime)
        {
            return dateTime.ToString("d MMMM yyyy hh:mm tt", new CultureInfo("ar-EG"))
                           .Replace("AM", "صباحاً")
                           .Replace("PM", "مساءً");
        }
    }
}

