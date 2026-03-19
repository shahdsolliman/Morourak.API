using System.Globalization;
using Morourak.Application.DTOs.Appointments;
using Morourak.Domain.Enums.Appointments;
using Morourak.Domain.Enums.Request;
using Morourak.Domain.Extensions;

namespace Morourak.API.Formatting;

public sealed class ArabicAppointmentFormatter : IAppointmentArabicFormatter
{
    private static readonly CultureInfo ArEg = new("ar-EG");
    private const string UnknownText = "غير محدد";

    public BookingConfirmationDto FormatBookingConfirmation(BookingConfirmationDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        // ================= Appointment formatting =================
        var date = ParseDateOnly(dto.Appointment.Date);
        var time = ParseTimeOnly(dto.Appointment.Time);

        var dateFormatted = FormatArabicDate(date);
        var timeFormatted = FormatArabicTime(time);

        dto.Appointment.Message = "تم الحجز بنجاح";
        dto.Appointment.DateFormatted = dateFormatted;
        dto.Appointment.TimeFormatted = timeFormatted;
        dto.Appointment.Date = dateFormatted;   // keep API contract consistent with previous behavior
        dto.Appointment.Time = timeFormatted;   // keep API contract consistent with previous behavior

        dto.Appointment.ServiceName = GetAppointmentTypeName(dto.Appointment.ServiceName);
        dto.Appointment.TrafficUnitName = NullOrDefault(dto.Appointment.TrafficUnitName);
        dto.Appointment.TrafficUnitAddress = NullOrDefault(dto.Appointment.TrafficUnitAddress);
        dto.Appointment.GovernorateName = NullOrDefault(dto.Appointment.GovernorateName);
        dto.Appointment.WorkingHours = NullOrDefault(dto.Appointment.WorkingHours);

        // ================= Service request formatting =================
        dto.ServiceRequest.ServiceType = GetServiceTypeName(dto.ServiceRequest.ServiceType);
        dto.ServiceRequest.Status = GetRequestStatusName(dto.ServiceRequest.Status);
        dto.ServiceRequest.PaymentStatus = GetPaymentStatusName(dto.ServiceRequest.PaymentStatus);

        dto.ServiceRequest.SubmittedAt = FormatArabicDate(ParseDateOnly(dto.ServiceRequest.SubmittedAt));

        // LastUpdatedAt might be empty if mapping layer changes; fallback to SubmittedAt.
        var lastUpdatedAtRaw = string.IsNullOrWhiteSpace(dto.ServiceRequest.LastUpdatedAt)
            ? dto.ServiceRequest.SubmittedAt
            : dto.ServiceRequest.LastUpdatedAt;

        dto.ServiceRequest.LastUpdatedAt = FormatArabicDate(ParseDateOnly(lastUpdatedAtRaw));

        return dto;
    }

    public AppointmentSummaryDto FormatAppointmentSummary(AppointmentSummaryDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        dto.ServiceName = FormatServiceName(dto.ServiceName);
        dto.Status = FormatAppointmentStatus(dto.Status);
        dto.Date = FormatArabicDate(dto.Date);

        return dto;
    }

    public AppointmentDetailsDto FormatAppointmentDetails(AppointmentDetailsDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        dto.ServiceName = FormatServiceName(dto.ServiceName);
        dto.Status = FormatAppointmentStatus(dto.Status);
        dto.Date = FormatArabicDate(dto.Date);

        dto.StartTime = FormatArabicTime(dto.StartTime);
        dto.EndTime = string.IsNullOrWhiteSpace(dto.EndTime) ? null : FormatArabicTime(dto.EndTime);

        dto.GovernorateName = NullOrDefault(dto.GovernorateName);
        dto.TrafficUnitName = NullOrDefault(dto.TrafficUnitName);

        dto.RequestNumberRelated = NullOrDefault(dto.RequestNumberRelated);
        dto.AssignedToUserId = NullOrDefault(dto.AssignedToUserId);

        return dto;
    }

    private static DateOnly ParseDateOnly(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return DateOnly.FromDateTime(DateTime.Today);

        // Handler layer produces yyyy-MM-dd.
        if (DateOnly.TryParseExact(raw, "yyyy-MM-dd", ArEg, DateTimeStyles.None, out var parsed))
            return parsed;

        // Fallback for unexpected inputs.
        if (DateOnly.TryParse(raw, ArEg, DateTimeStyles.None, out var parsed2))
            return parsed2;

        return DateOnly.FromDateTime(DateTime.Today);
    }

    private static TimeOnly ParseTimeOnly(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return new TimeOnly(9, 0);

        // Handler layer produces HH:mm.
        if (TimeOnly.TryParseExact(raw, "HH:mm", ArEg, DateTimeStyles.None, out var parsed))
            return parsed;

        if (TimeOnly.TryParse(raw, ArEg, DateTimeStyles.None, out var parsed2))
            return parsed2;

        return new TimeOnly(9, 0);
    }

    private static string FormatArabicDate(DateOnly date)
        => date.ToString("d MMMM yyyy", ArEg);

    private static string FormatArabicDate(string rawDate)
    {
        if (string.IsNullOrWhiteSpace(rawDate))
            return FormatArabicDate(DateOnly.FromDateTime(DateTime.Today));

        // Expecting ISO "yyyy-MM-dd" coming from Application layer.
        if (DateOnly.TryParseExact(rawDate, "yyyy-MM-dd", ArEg, DateTimeStyles.None, out var parsed))
            return FormatArabicDate(parsed);

        if (DateOnly.TryParse(rawDate, ArEg, DateTimeStyles.None, out var parsed2))
            return FormatArabicDate(parsed2);

        return UnknownText;
    }

    private static string FormatArabicTime(TimeOnly time)
        => time
            .ToString("hh:mm tt", CultureInfo.GetCultureInfo("en-US"))
            .Replace("AM", "صباحاً")
            .Replace("PM", "مساءً");

    private static string FormatArabicTime(string rawTime)
    {
        if (string.IsNullOrWhiteSpace(rawTime))
            return UnknownText;

        // Expecting "HH:mm" (24-hour).
        if (TimeSpan.TryParseExact(
                rawTime,
                "hh\\:mm",
                CultureInfo.InvariantCulture,
                out var timeSpan))
        {
            return FormatArabicTime(TimeOnly.FromTimeSpan(timeSpan));
        }

        return UnknownText;
    }

    private static string GetAppointmentTypeName(string rawAppointmentType)
    {
        if (Enum.TryParse<AppointmentType>(rawAppointmentType, true, out var appointmentType))
        {
            return appointmentType switch
            {
                AppointmentType.Medical => "كشف طبي",
                AppointmentType.Driving => "اختبار قيادة",
                AppointmentType.Technical => "فحص فني",
                _ => UnknownText
            };
        }

        return UnknownText;
    }

    private static string GetServiceTypeName(string rawServiceType)
    {
        if (!Enum.TryParse<ServiceType>(rawServiceType, true, out var serviceType))
            return UnknownText;

        return serviceType switch
        {
            ServiceType.ExaminationTechnical => "فحص فني",
            ServiceType.ExaminationDriving => "اختبار قيادة",
            _ => serviceType.GetDisplayName()
        };
    }

    private static string GetRequestStatusName(string rawStatus)
    {
        if (Enum.TryParse<RequestStatus>(rawStatus, true, out var status))
            return status.GetDisplayName();

        return UnknownText;
    }

    private static string GetPaymentStatusName(string rawPaymentStatus)
    {
        if (!Enum.TryParse<PaymentStatus>(rawPaymentStatus, true, out var paymentStatus))
            return UnknownText;

        return paymentStatus switch
        {
            PaymentStatus.Pending => "قيد الانتظار",
            PaymentStatus.Paid => "مدفوع",
            PaymentStatus.Failed => "فشل",
            _ => UnknownText
        };
    }

    private static string NullOrDefault(string? value)
        => string.IsNullOrWhiteSpace(value) ? UnknownText : value;

    private static string FormatServiceName(string rawServiceName)
        => GetAppointmentTypeName(rawServiceName);

    private static string FormatAppointmentStatus(string rawStatus)
    {
        if (!Enum.TryParse<AppointmentStatus>(rawStatus, true, out var status))
            return UnknownText;

        return status switch
        {
            AppointmentStatus.Pending => "قيد الانتظار",
            AppointmentStatus.Scheduled => "محجوز",
            AppointmentStatus.Completed => "مكتمل",
            AppointmentStatus.Cancelled => "ملغى",
            AppointmentStatus.Passed => "ناجح",
            AppointmentStatus.Available => "متاح",
            AppointmentStatus.Failed => "راسب",
            _ => UnknownText
        };
    }
}

