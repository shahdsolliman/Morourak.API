using Morourak.Application.CQRS.Appointment.Commands.CreateAppointment;
using Morourak.Application.Exceptions;
using Morourak.Application.Interfaces;
using Morourak.Application.Interfaces.DomainServices;
using Morourak.Domain.Entities;
using AppointmentEntity = Morourak.Domain.Entities.Appointment;
using Morourak.Domain.Enums.Appointments;
using Morourak.Domain.Enums.Request;

namespace Morourak.Application.DomainServices.Appointment;

public sealed class AppointmentDomainService : IAppointmentDomainService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRequestDomainService _requestDomainService;

    private static readonly TimeOnly WorkStart = new(9, 0);
    private static readonly TimeOnly WorkEnd = new(14, 0);
    private const int SlotDurationMinutes = 30;

    public AppointmentDomainService(
        IUnitOfWork unitOfWork,
        IRequestDomainService requestDomainService)
    {
        _unitOfWork = unitOfWork;
        _requestDomainService = requestDomainService;
    }

    public async Task<BookingConfirmationContext> ConfirmBookingAsync(
        string nationalId,
        string serviceType,
        DateOnly date,
        TimeOnly time,
        int governorateId,
        int trafficUnitId,
        CancellationToken cancellationToken)
    {
        var appointmentType = MapToAppointmentType(serviceType);

        ValidateWorkingHours(time);

        var governorate = await ValidateGovernorate(governorateId);
        var trafficUnit = await ValidateTrafficUnit(governorateId, trafficUnitId);

        var appointmentRepo = _unitOfWork.Repository<AppointmentEntity>();

        // Slot availability: same unit + date + type + exact time + not cancelled.
        var slotTaken = await appointmentRepo.FindAsync(a =>
            a.TrafficUnitId == trafficUnitId &&
            a.Date == date &&
            a.Type == appointmentType &&
            a.StartTime == time &&
            a.Status != AppointmentStatus.Cancelled);

        if (slotTaken.Any())
            throw new ValidationException(
                "هذا الموعد محجوز بالفعل لهذه الخدمة.",
                "SLOT_UNAVAILABLE");

        // One active appointment per citizen per (date,time).
        var overlappingActiveAppointments = (await appointmentRepo.FindAsync(a =>
                a.CitizenNationalId == nationalId &&
                a.Date == date &&
                a.StartTime == time))
            .Where(a => IsActiveAppointmentStatus(a.Status))
            .ToList();

        if (overlappingActiveAppointments.Any())
            throw new ValidationException(
                "لا يمكن حجز أكثر من موعد نشط في نفس التاريخ والتوقيت.",
                "CITIZEN_TIME_CONFLICT");

        var serviceRequest = await _requestDomainService.FindPrimaryServiceRequestAsync(
            nationalId,
            appointmentType,
            cancellationToken);

        if (serviceRequest == null)
            throw new ValidationException(
                "لا يوجد طلب خدمة نشط لهذا المواطن.",
                "APPLICATION_NOT_FOUND");

        var assignedToUserId = ResolveAssignedToUserId(appointmentType);

        var now = DateTime.UtcNow;
        var appointment = new AppointmentEntity
        {
            ApplicationId = serviceRequest.ReferenceId,
            CitizenNationalId = nationalId,
            Date = date,
            StartTime = time,
            EndTime = time.AddMinutes(SlotDurationMinutes),
            Status = AppointmentStatus.Scheduled,
            Type = appointmentType,
            RequestNumber = serviceRequest.RequestNumber,
            GovernorateId = governorateId,
            TrafficUnitId = trafficUnitId,
            CreatedAt = now
        };

        await appointmentRepo.AddAsync(appointment);
        await _unitOfWork.CommitAsync();

        return new BookingConfirmationContext(
            appointment: appointment,
            serviceRequest: serviceRequest,
            governorate: governorate,
            trafficUnit: trafficUnit,
            appointmentType: appointmentType,
            assignedToUserId: assignedToUserId);
    }

    private static AppointmentType MapToAppointmentType(string serviceType)
    {
        var normalized = serviceType.Trim();

        if (normalized.Equals("كشف طبي", StringComparison.OrdinalIgnoreCase))
            return AppointmentType.Medical;

        if (normalized.Equals("فحص فني", StringComparison.OrdinalIgnoreCase))
            return AppointmentType.Technical;

        if (normalized.Equals("اختبار قيادة", StringComparison.OrdinalIgnoreCase))
            return AppointmentType.Driving;

        if (normalized.Contains("مركبة", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals(nameof(ServiceType.VehicleLicenseIssue), StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals(nameof(ServiceType.VehicleLicenseRenewal), StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals(nameof(ServiceType.VehicleLicenseReplacementLost), StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals(nameof(ServiceType.VehicleLicenseReplacementDamaged), StringComparison.OrdinalIgnoreCase))
        {
            return AppointmentType.Technical;
        }

        if (normalized.Contains("قيادة", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals(nameof(ServiceType.DrivingLicenseIssue), StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals(nameof(ServiceType.DrivingLicenseRenewal), StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals(nameof(ServiceType.DrivingLicenseReplacementLost), StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals(nameof(ServiceType.DrivingLicenseReplacementDamaged), StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals(nameof(ServiceType.DrivingLicenseUpgrade), StringComparison.OrdinalIgnoreCase))
        {
            return AppointmentType.Driving;
        }

        throw new ValidationException(
            "نوع الخدمة غير مدعوم.",
            "INVALID_SERVICE_TYPE");
    }

    private static bool IsActiveAppointmentStatus(AppointmentStatus status)
    {
        return status == AppointmentStatus.Pending || status == AppointmentStatus.Scheduled;
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

    private static void ValidateWorkingHours(TimeOnly time)
    {
        if (time < WorkStart || time >= WorkEnd)
            throw new ValidationException(
                "الموعد خارج ساعات العمل الرسمية.",
                "INVALID_WORKING_HOURS");
    }

    private async Task<Governorate> ValidateGovernorate(int id)
    {
        var gov = await _unitOfWork.Repository<Governorate>()
            .GetByIdAsync(id);

        if (gov == null)
            throw new ValidationException(
                "المحافظة المختارة غير موجودة.",
                "GOVERNORATE_NOT_FOUND");

        return gov;
    }

    private async Task<TrafficUnit> ValidateTrafficUnit(int govId, int unitId)
    {
        var unit = await _unitOfWork.Repository<TrafficUnit>()
            .GetAsync(u => u.Id == unitId && u.GovernorateId == govId);

        if (unit == null)
            throw new ValidationException(
                "وحدة المرور المختارة غير موجودة في هذه المحافظة.",
                "TRAFFIC_UNIT_NOT_FOUND");

        return unit;
    }
}

