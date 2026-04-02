using Morourak.Application.Exceptions;
using Morourak.Application.DTOs.Appointments;
using Morourak.Domain.Enums.Appointments;

namespace Morourak.API.Extensions.Appointments;

internal static class AppointmentTypeInputResolver
{
    public static AppointmentType Resolve(ConfirmAppointmentRequestDto request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        if (request.AppointmentType.HasValue)
        {
            if (!string.IsNullOrWhiteSpace(request.ServiceType) &&
                LegacyAppointmentTypeMapper.TryMap(request.ServiceType, out var legacy) &&
                legacy != request.AppointmentType.Value)
            {
                throw new ValidationException("تعارض بين نوع الموعد والقيمة القديمة لنوع الخدمة.", "CONFLICTING_APPOINTMENT_TYPE");
            }

            return request.AppointmentType.Value;
        }

        if (LegacyAppointmentTypeMapper.TryMap(request.ServiceType, out var mapped))
            return mapped;

        throw new ValidationException("نوع الموعد غير مدعوم.", "INVALID_SERVICE_TYPE");
    }
}

