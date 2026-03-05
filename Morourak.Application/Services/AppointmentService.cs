using Morourak.Application.DTOs.Appointments;
using Morourak.Application.Interfaces;
using Morourak.Application.Interfaces.Services;
using Morourak.Domain.Entities;
using Morourak.Domain.Enums.Appointments;
using Morourak.Domain.Enums.Request;
using Morourak.Domain.Extensions;
using AppEx = Morourak.Application.Exceptions;
using System.Globalization;

namespace Morourak.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IUnitOfWork _unitOfWork;

        private static readonly TimeOnly WorkStart = new(9, 0);
        private static readonly TimeOnly WorkEnd = new(14, 0);
        private const int SlotDurationMinutes = 30;

        public AppointmentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<AppointmentDto>> GetAvailableSlotsAsync(
            DateOnly date,
            AppointmentType type,
            int trafficUnitId)
        {
            if (date < DateOnly.FromDateTime(DateTime.Today))
                throw new AppEx.ValidationException(
                    "لا يمكن عرض مواعيد لتاريخ سابق.",
                    "INVALID_PAST_DATE");

            var repo = _unitOfWork.Repository<Appointment>();

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

                slots.Add(new AppointmentDto
                {
                    Type = type,
                    TypeName = GetAppointmentTypeName(type),
                    ServiceName = GetAppointmentTypeName(type),
                    AssignedToUserId = ResolveAssignedToUserId(type),
                    Date = date,
                    DateFormatted = FormatArabicDate(date),
                    StartTime = time,
                    TimeFormatted = FormatArabicTime(time),
                    EndTime = time.AddMinutes(SlotDurationMinutes),
                    Status = AppointmentStatus.Available,
                    GovernorateId = 0,
                    TrafficUnitId = trafficUnitId,
                    GovernorateName = "غير محدد",
                    TrafficUnitName = "غير محدد",
                    CreatedAt = FormatArabicDateTime(DateTime.Now),
                });
            }

            return slots;
        }

        public async Task<BookingConfirmationDto> ConfirmBookingAsync(
            string nationalId,
            ConfirmAppointmentRequestDto request)
        {
            var appointmentType = MapToAppointmentType(request.ServiceType);

            ValidateWorkingHours(request.Time);

            var governorate = await ValidateGovernorate(request.GovernorateId);
            var trafficUnit = await ValidateTrafficUnit(
                request.GovernorateId,
                request.TrafficUnitId);

            var appointmentRepo = _unitOfWork.Repository<Appointment>();

            var slotTaken = await appointmentRepo.FindAsync(a =>
                a.TrafficUnitId == request.TrafficUnitId &&
                a.Date == request.Date &&
                a.Type == appointmentType &&
                a.StartTime == request.Time &&
                a.Status != AppointmentStatus.Cancelled);

            if (slotTaken.Any())
                throw new AppEx.ValidationException(
                    "هذا الموعد محجوز بالفعل لهذه الخدمة.",
                    "SLOT_UNAVAILABLE");

            var overlappingActiveAppointment = (await appointmentRepo.FindAsync(a =>
                    a.CitizenNationalId == nationalId &&
                    a.Date == request.Date &&
                    a.StartTime == request.Time))
                .Where(a => IsActiveAppointmentStatus(a.Status))
                .ToList();

            if (overlappingActiveAppointment.Any())
                throw new AppEx.ValidationException(
                    "لا يمكن حجز أكثر من موعد نشط في نفس التاريخ والتوقيت.",
                    "CITIZEN_TIME_CONFLICT");

            var serviceRequest = await FindPrimaryServiceRequestAsync(nationalId, appointmentType);
            if (serviceRequest == null)
                throw new AppEx.ValidationException(
                    "APPLICATION_NOT_FOUND",
                    "APPLICATION_NOT_FOUND");

            var assignedToUserId = ResolveAssignedToUserId(appointmentType);
            var now = DateTime.UtcNow;

            var appointment = new Appointment
            {
                ApplicationId = serviceRequest.ReferenceId,
                CitizenNationalId = nationalId,
                Date = request.Date,
                StartTime = request.Time,
                EndTime = request.Time.AddMinutes(SlotDurationMinutes),
                Status = AppointmentStatus.Scheduled,
                Type = appointmentType,
                RequestNumber = serviceRequest.RequestNumber,
                GovernorateId = request.GovernorateId,
                TrafficUnitId = request.TrafficUnitId,
                CreatedAt = now
            };

            await appointmentRepo.AddAsync(appointment);
            await _unitOfWork.CommitAsync();

            return BuildConfirmationResponse(
                appointment,
                serviceRequest,
                governorate,
                trafficUnit,
                assignedToUserId);
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

            throw new AppEx.ValidationException(
                "نوع الخدمة غير مدعوم.",
                "INVALID_SERVICE_TYPE");
        }

        private static bool IsActiveAppointmentStatus(AppointmentStatus status)
        {
            return status == AppointmentStatus.Pending || status == AppointmentStatus.Scheduled;
        }

        private async Task<ServiceRequest?> FindPrimaryServiceRequestAsync(string nationalId, AppointmentType appointmentType)
        {
            var serviceRequestRepo = _unitOfWork.Repository<ServiceRequest>();
            var primaryServiceTypes = GetPrimaryServiceTypesForAppointment(appointmentType);

            var relatedRequests = await serviceRequestRepo.FindAsync(sr =>
                sr.CitizenNationalId == nationalId &&
                sr.ReferenceId > 0 &&
                primaryServiceTypes.Contains(sr.ServiceType) &&
                sr.Status != RequestStatus.Cancelled);

            return relatedRequests
                .Where(sr => sr.RequestNumber.StartsWith($"{GetRequestPrefix(sr.ServiceType)}-", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(sr => sr.SubmittedAt)
                .ThenByDescending(sr => sr.LastUpdatedAt ?? sr.SubmittedAt)
                .FirstOrDefault();
        }

        private static ServiceType[] GetPrimaryServiceTypesForAppointment(AppointmentType appointmentType)
        {
            return appointmentType switch
            {
                AppointmentType.Technical =>
                [
                    ServiceType.VehicleLicenseIssue,
                    ServiceType.VehicleLicenseRenewal,
                    ServiceType.VehicleLicenseReplacementLost,
                    ServiceType.VehicleLicenseReplacementDamaged
                ],
                AppointmentType.Medical or AppointmentType.Driving =>
                [
                    ServiceType.DrivingLicenseIssue,
                    ServiceType.DrivingLicenseRenewal,
                    ServiceType.DrivingLicenseReplacementLost,
                    ServiceType.DrivingLicenseReplacementDamaged,
                    ServiceType.DrivingLicenseUpgrade
                ],
                _ => Array.Empty<ServiceType>()
            };
        }

        private static string GetRequestPrefix(ServiceType serviceType)
        {
            return serviceType switch
            {
                ServiceType.DrivingLicenseIssue => "DL",
                ServiceType.DrivingLicenseRenewal => "DR",
                ServiceType.VehicleLicenseIssue => "VL",
                ServiceType.VehicleLicenseRenewal => "VR",
                ServiceType.VehicleLicenseReplacementLost => "RPL",
                ServiceType.VehicleLicenseReplacementDamaged => "RPD",
                ServiceType.DrivingLicenseReplacementLost => "EL",
                ServiceType.DrivingLicenseReplacementDamaged => "ED",
                _ => "SR"
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

        private static void ValidateWorkingHours(TimeOnly time)
        {
            if (time < WorkStart || time >= WorkEnd)
                throw new AppEx.ValidationException(
                    "الموعد خارج ساعات العمل الرسمية.",
                    "INVALID_WORKING_HOURS");
        }

        private async Task<Governorate> ValidateGovernorate(int id)
        {
            var gov = await _unitOfWork.Repository<Governorate>()
                .GetByIdAsync(id);

            if (gov == null)
                throw new AppEx.ValidationException(
                    "المحافظة المختارة غير موجودة.",
                    "GOVERNORATE_NOT_FOUND");

            return gov;
        }

        private async Task<TrafficUnit> ValidateTrafficUnit(int govId, int unitId)
        {
            var unit = await _unitOfWork.Repository<TrafficUnit>()
                .GetAsync(u => u.Id == unitId && u.GovernorateId == govId);

            if (unit == null)
                throw new AppEx.ValidationException(
                    "وحدة المرور المختارة غير موجودة في هذه المحافظة.",
                    "TRAFFIC_UNIT_NOT_FOUND");

            return unit;
        }

        private static BookingConfirmationDto BuildConfirmationResponse(
            Appointment appointment,
            ServiceRequest serviceRequest,
            Governorate governorate,
            TrafficUnit trafficUnit,
            string assignedToUserId)
        {
            var dateFormatted = FormatArabicDate(appointment.Date);
            var timeFormatted = FormatArabicTime(appointment.StartTime);

            return new BookingConfirmationDto
            {
                Appointment = new BookingAppointmentDto
                {
                    Message = "تم الحجز بنجاح",
                    BookingNumber = appointment.Id.ToString(),
                    ApplicationId = appointment.ApplicationId,
                    RequestNumber = serviceRequest.RequestNumber,
                    ServiceName = GetAppointmentTypeName(appointment.Type),
                    Date = dateFormatted,
                    Time = timeFormatted,
                    DateFormatted = dateFormatted,
                    TimeFormatted = timeFormatted,
                    TrafficUnitName = NullOrDefault(trafficUnit.Name),
                    TrafficUnitAddress = NullOrDefault(trafficUnit.Address),
                    GovernorateName = NullOrDefault(governorate.Name),
                    WorkingHours = NullOrDefault(trafficUnit.WorkingHours),
                    AssignedToUserId = assignedToUserId
                },
                ServiceRequest = new BookingServiceRequestDto
                {
                    RequestNumber = serviceRequest.RequestNumber,
                    CitizenNationalId = serviceRequest.CitizenNationalId,
                    ServiceType = GetServiceTypeName(serviceRequest.ServiceType),
                    Status = GetRequestStatusName(serviceRequest.Status),
                    PaymentStatus = GetPaymentStatusName(serviceRequest.PaymentStatus),
                    SubmittedAt = FormatArabicDate(serviceRequest.SubmittedAt),
                    LastUpdatedAt = FormatArabicDate(serviceRequest.LastUpdatedAt ?? serviceRequest.SubmittedAt)
                }
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

        private static string GetServiceTypeName(ServiceType serviceType)
        {
            return serviceType switch
            {
                ServiceType.ExaminationTechnical => "فحص فني",
                ServiceType.ExaminationDriving => "اختبار قيادة",
                _ => serviceType.GetDisplayName()
            };
        }

        private static string GetRequestStatusName(RequestStatus status)
        {
            return status.GetDisplayName();
        }

        private static string GetPaymentStatusName(PaymentStatus paymentStatus)
        {
            return paymentStatus switch
            {
                PaymentStatus.Pending => "قيد الانتظار",
                PaymentStatus.Paid => "مدفوع",
                PaymentStatus.Failed => "فشل",
                _ => "غير محدد"
            };
        }

        private static string NullOrDefault(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? "غير محدد" : value;
        }

        private static string FormatArabicDate(DateOnly date)
        {
            return date.ToString("d MMMM yyyy", new CultureInfo("ar-EG"));
        }

        private static string FormatArabicDate(DateTime dateTime)
        {
            return DateOnly.FromDateTime(dateTime).ToString("d MMMM yyyy", new CultureInfo("ar-EG"));
        }

        private static string FormatArabicTime(TimeOnly time)
        {
            return time
                .ToString("hh:mm tt", new CultureInfo("en-US"))
                .Replace("AM", "صباحاً")
                .Replace("PM", "مساءً");
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

        public async Task UpdateStatusAsync(
            int applicationId,
            AppointmentType type,
            bool passed,
            string? notes,
            string staffUserId)
        {
            var repo = _unitOfWork.Repository<Appointment>();

            var appointment = (await repo.FindAsync(a =>
                    a.ApplicationId == applicationId &&
                    a.Type == type &&
                    a.Status == AppointmentStatus.Scheduled))
                .OrderByDescending(a => a.Date)
                .ThenByDescending(a => a.StartTime)
                .FirstOrDefault();

            if (appointment == null)
                throw new AppEx.ValidationException(
                    $"No scheduled {type} appointment found for application {applicationId}.",
                    "APPOINTMENT_NOT_FOUND");

            appointment.Status = passed ? AppointmentStatus.Passed : AppointmentStatus.Failed;
            appointment.UpdatedAt = DateTime.UtcNow;

            repo.Update(appointment);

            var drivingRepo = _unitOfWork.Repository<DrivingLicenseApplication>();
            var drivingApp = await drivingRepo.GetByIdAsync(applicationId);

            if (drivingApp != null)
            {
                if (type == AppointmentType.Medical) drivingApp.MedicalExaminationPassed = passed;
                if (type == AppointmentType.Driving) drivingApp.DrivingTestPassed = passed;
                drivingRepo.Update(drivingApp);
            }
            else
            {
                var renewalRepo = _unitOfWork.Repository<RenewalApplication>();
                var renewal = await renewalRepo.GetByIdAsync(applicationId);

                if (renewal != null && type == AppointmentType.Medical)
                {
                    renewal.MedicalExaminationPassed = passed;
                    renewalRepo.Update(renewal);
                }
            }

            var serviceRequestRepo = _unitOfWork.Repository<ServiceRequest>();
            var serviceRequest = (await serviceRequestRepo
                .FindAsync(sr => sr.RequestNumber == appointment.RequestNumber))
                .FirstOrDefault();

            if (serviceRequest != null)
            {
                var relatedAppointments = await repo.FindAsync(a => a.ApplicationId == applicationId);

                bool medicalPassed = relatedAppointments.Any(a => a.Type == AppointmentType.Medical && a.Status == AppointmentStatus.Passed);
                bool drivingPassed = relatedAppointments.Any(a => a.Type == AppointmentType.Driving && a.Status == AppointmentStatus.Passed);
                bool technicalPassed = relatedAppointments.Any(a => a.Type == AppointmentType.Technical && a.Status == AppointmentStatus.Passed);

                bool allPassed = medicalPassed && drivingPassed && technicalPassed;

                serviceRequest.Status = allPassed ? RequestStatus.Passed : RequestStatus.Pending;
                serviceRequest.LastUpdatedAt = DateTime.UtcNow;
                serviceRequestRepo.Update(serviceRequest);
            }

            await _unitOfWork.CommitAsync();
        }

        private AppointmentDto MapToDto(Appointment appointment)
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
                RequestNumber = appointment.RequestNumber,
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

        private static string FormatArabicDateTime(DateTime dateTime)
        {
            return dateTime.ToString("d MMMM yyyy hh:mm tt", new CultureInfo("ar-EG"))
                           .Replace("AM", "صباحاً")
                           .Replace("PM", "مساءً");
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

    }
}
