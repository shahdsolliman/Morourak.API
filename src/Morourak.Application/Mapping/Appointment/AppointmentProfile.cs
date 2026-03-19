using AutoMapper;
using Morourak.Application.CQRS.Appointment.Commands.CreateAppointment;
using Morourak.Application.DTOs.Appointments;
using Morourak.Domain.Enums.Appointments;
using System.Globalization;

namespace Morourak.Application.Mapping.Appointment;

public sealed class AppointmentProfile : Profile
{
    public AppointmentProfile()
    {
        CreateMap<BookingConfirmationContext, BookingAppointmentDto>()
            .ForMember(d => d.Message, opt => opt.MapFrom(_ => string.Empty))
            .ForMember(d => d.BookingNumber, opt => opt.MapFrom(src => src.Appointment.Id.ToString()))
            .ForMember(d => d.ApplicationId, opt => opt.MapFrom(src => src.Appointment.ApplicationId))
            .ForMember(d => d.RequestNumber, opt => opt.MapFrom(src => src.ServiceRequest.RequestNumber))
            .ForMember(d => d.ServiceName, opt => opt.MapFrom(src => src.AppointmentType.ToString()))
            .ForMember(d => d.Date, opt => opt.MapFrom(src =>
                src.Appointment.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)))
            .ForMember(d => d.Time, opt => opt.MapFrom(src =>
                src.Appointment.StartTime.ToString("HH:mm", CultureInfo.InvariantCulture)))
            .ForMember(d => d.DateFormatted, opt => opt.MapFrom(src => 
                ToArabicDate(src.Appointment.Date)))
            .ForMember(d => d.TimeFormatted, opt => opt.MapFrom(src => 
                ToArabicTime(src.Appointment.StartTime)))
            .ForMember(d => d.TrafficUnitName, opt => opt.MapFrom(src => 
                src.TrafficUnit.Name.Contains("العاشر") ? "وحدة مرور العاشر من رمضان" : (src.TrafficUnit.Name ?? string.Empty)))
            .ForMember(d => d.TrafficUnitAddress, opt => opt.MapFrom(src => 
                src.TrafficUnit.Name.Contains("العاشر") ? "شارع التسعين , العاشر من رمضان , الشرقية" : (src.TrafficUnit.Address ?? string.Empty)))
            .ForMember(d => d.GovernorateName, opt => opt.MapFrom(src => src.Governorate.Name ?? string.Empty))
            .ForMember(d => d.WorkingHours, opt => opt.MapFrom(src => 
                src.TrafficUnit.Name.Contains("العاشر") ? "9 ص الي 3 م (الاحد -الخميس)" : (src.TrafficUnit.WorkingHours ?? string.Empty)))
            .ForMember(d => d.AssignedToUserId, opt => opt.MapFrom(src => src.AssignedToUserId));

        CreateMap<BookingConfirmationContext, BookingServiceRequestDto>()
            .ForMember(d => d.RequestNumber, opt => opt.MapFrom(src => src.ServiceRequest.RequestNumber))
            .ForMember(d => d.CitizenNationalId, opt => opt.MapFrom(src => src.ServiceRequest.CitizenNationalId))
            .ForMember(d => d.ServiceType, opt => opt.MapFrom(src => src.ServiceRequest.ServiceType.ToString()))
            .ForMember(d => d.Status, opt => opt.MapFrom(src => src.ServiceRequest.Status.ToString()))
            .ForMember(d => d.PaymentStatus, opt => opt.MapFrom(src => src.ServiceRequest.PaymentStatus.ToString()))
            .ForMember(d => d.SubmittedAt, opt => opt.MapFrom(src =>
                src.ServiceRequest.SubmittedAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)))
            .ForMember(d => d.LastUpdatedAt, opt => opt.MapFrom(src =>
                (src.ServiceRequest.LastUpdatedAt ?? src.ServiceRequest.SubmittedAt)
                    .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));

        CreateMap<BookingConfirmationContext, BookingConfirmationDto>()
            .ForMember(d => d.Appointment, opt => opt.MapFrom(src => src))
            .ForMember(d => d.ServiceRequest, opt => opt.MapFrom(src => src));

        // Raw mapping for appointment read models (no Arabic formatting here).
        CreateMap<Morourak.Domain.Entities.Appointment, AppointmentDto>()
            .ForMember(d => d.TypeName, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(d => d.ServiceName, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(d => d.DateFormatted, opt => opt.MapFrom(_ => string.Empty))
            .ForMember(d => d.TimeFormatted, opt => opt.MapFrom(_ => string.Empty))
            .ForMember(d => d.CreatedAt, opt => opt.MapFrom(src =>
                src.CreatedAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)))
            .ForMember(d => d.CompletedAt, opt => opt.MapFrom(src =>
                src.UpdatedAt.HasValue
                    ? src.UpdatedAt.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                    : string.Empty))
            .ForMember(d => d.RequestNumberRelated, opt => opt.MapFrom(src => src.RequestNumber))
            .ForMember(d => d.AssignedToUserId, opt => opt.MapFrom(src => ResolveAssignedToUserId(src.Type)))
            .ForMember(d => d.EndTime, opt => opt.MapFrom(src => (TimeOnly?)src.EndTime))
            .ForMember(d => d.GovernorateName, opt => opt.MapFrom(src => src.Governorate != null ? src.Governorate.Name : string.Empty))
            .ForMember(d => d.TrafficUnitName, opt => opt.MapFrom(src => src.TrafficUnit != null ? src.TrafficUnit.Name : string.Empty))
            .ForMember(d => d.GovernorateId, opt => opt.MapFrom(src => src.GovernorateId))
            .ForMember(d => d.TrafficUnitId, opt => opt.MapFrom(src => src.TrafficUnitId));
    }

    private static string ToArabicDate(DateOnly date)
    {
        string[] months = {
            "يناير", "فبراير", "مارس", "ابريل", "مايو", "يونيو",
            "يوليو", "اغسطس", "سبتمبر", "اكتوبر", "نوفمبر", "ديسمبر"
        };

        return $"{date.Day} {months[date.Month - 1]} {date.Year}";
    }

    private static string ToArabicTime(TimeOnly time)
    {
        var hour = time.Hour;
        var minute = time.Minute;
        var suffix = hour >= 12 ? "مساءا" : "صباحا";
        
        var displayHour = hour % 12;
        if (displayHour == 0) displayHour = 12;

        return $"{displayHour}:{minute:D2} {suffix}";
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
}

