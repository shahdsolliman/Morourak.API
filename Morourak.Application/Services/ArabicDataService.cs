using Morourak.Application.DTOs.Appointments;
using Morourak.Application.Interfaces;
using Morourak.Application.Interfaces.Services;
using Morourak.Domain.Entities;
using Morourak.Domain.Enums.Appointments;
using Morourak.Application.Exceptions;
using System.Globalization;

namespace Morourak.Application.Services
{
    public class ArabicDataService : IArabicDataService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ArabicDataService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ArabicAppointmentDto> GetArabicAppointmentByIdAsync(int appointmentId)
        {
            var appointment = await _unitOfWork.Repository<Appointment>().GetAsync(
                a => a.Id == appointmentId,
                a => a.Governorate!,
                a => a.TrafficUnit!);

            if (appointment == null)
            {
                throw new NotFoundException($"Appointment with ID {appointmentId} not found.");
            }

            return await MapToArabicDtoAsync(appointment);
        }

        public async Task<IEnumerable<ArabicAppointmentDto>> GetArabicAppointmentsByRoleAsync(string role, string? userId = null)
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
                return Enumerable.Empty<ArabicAppointmentDto>();

            var appointments = await _unitOfWork.Repository<Appointment>().FindAsync(a =>
                a.Type == type.Value &&
                a.Status != AppointmentStatus.Cancelled,
                a => a.Governorate!,
                a => a.TrafficUnit!);

            var result = new List<ArabicAppointmentDto>();
            foreach (var appointment in appointments)
            {
                result.Add(await MapToArabicDtoAsync(appointment));
            }

            return result;
        }

        private async Task<ArabicAppointmentDto> MapToArabicDtoAsync(Appointment appointment)
        {
            var citizen = await _unitOfWork.Repository<CitizenRegistry>().GetAsync(
                c => c.NationalId == appointment.CitizenNationalId);

            string applicantName = citizen != null
                ? $"{citizen.FirstName} {citizen.LastName}"
                : "غير معروف";

            return new ArabicAppointmentDto
            {
                RequestNumber = appointment.RequestNumber,
                ApplicantName = applicantName,
                NationalId = appointment.CitizenNationalId,
                TestType = GetArabicAppointmentTypeName(appointment.Type),
                ReservationDateTime = FormatArabicDateTime(appointment.Date, appointment.StartTime)
            };
        }

        private static string GetArabicAppointmentTypeName(AppointmentType type)
        {
            return type switch
            {
                AppointmentType.Medical => "كشف طبي",
                AppointmentType.Technical => "فحص فني",
                AppointmentType.Driving => "اختبار قيادة عملي",
                _ => "غير محدد"
            };
        }

        private static string FormatArabicDateTime(DateOnly date, TimeOnly time)
        {
            var dateTime = date.ToDateTime(time);
            return dateTime.ToString("d MMMM yyyy hh:mm tt", new CultureInfo("ar-EG"))
                           .Replace("AM", "صباحاً")
                           .Replace("PM", "مساءً");
        }
    }
}
