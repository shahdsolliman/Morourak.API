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
        private readonly IRequestNumberGenerator _generator;

        private static readonly TimeOnly WorkStart = new(9, 0);
        private static readonly TimeOnly WorkEnd = new(14, 0);
        private const int SlotDurationMinutes = 30;

        public AppointmentService(IUnitOfWork unitOfWork,
                                  IRequestNumberGenerator generator)
        {
            _unitOfWork = unitOfWork;
            _generator = generator;
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
                    CreatedAt = DateTime.UtcNow
                });
            }

            return slots;
        }

        public async Task<BookingConfirmationDto> ConfirmBookingAsync(
            string nationalId,
            ConfirmAppointmentRequestDto request)
        {
            var appointmentType = MapToAppointmentType(request.ServiceType);
            var serviceType = MapToServiceType(request.ServiceType);

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

            var duplicateType = await appointmentRepo.FindAsync(a =>
                a.CitizenNationalId == nationalId &&
                a.Type == appointmentType &&
                IsActiveAppointmentStatus(a.Status));

            if (duplicateType.Any())
                throw new AppEx.ValidationException(
                    "لديك موعد نشط بالفعل لنفس نوع الخدمة.",
                    "DUPLICATE_APPOINTMENT_TYPE");

            var overlappingDifferentType = await appointmentRepo.FindAsync(a =>
                a.CitizenNationalId == nationalId &&
                a.Date == request.Date &&
                a.StartTime == request.Time &&
                a.Type != appointmentType &&
                IsActiveAppointmentStatus(a.Status));

            if (overlappingDifferentType.Any())
                throw new AppEx.ValidationException(
                    "لا يمكن حجز نوعين مختلفين في نفس التاريخ والتوقيت.",
                    "CITIZEN_TIME_CONFLICT");

            var applicationId = await GetLinkedApplicationIdAsync(nationalId, serviceType);
            if (applicationId <= 0)
                throw new AppEx.ValidationException(
                    "لا يوجد طلب مرتبط صالح لإتمام الحجز.",
                    "APPLICATION_NOT_FOUND");

            var now = DateTime.UtcNow;
            var serviceRequestRepo = _unitOfWork.Repository<ServiceRequest>();

            var serviceRequest = new ServiceRequest
            {
                RequestNumber = await _generator.GenerateAsync(serviceType),
                CitizenNationalId = nationalId,
                ServiceType = serviceType,
                ReferenceId = applicationId,
                Status = RequestStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                SubmittedAt = now,
                LastUpdatedAt = now
            };

            await serviceRequestRepo.AddAsync(serviceRequest);

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
                Notes = null,
                AssignedToUserId = null,
                CreatedAt = now
            };

            await appointmentRepo.AddAsync(appointment);
            await _unitOfWork.CommitAsync();

            return BuildConfirmationResponse(
                appointment,
                serviceRequest,
                governorate,
                trafficUnit);
        }

        private static AppointmentType MapToAppointmentType(string serviceType) =>
            serviceType.Trim() switch
            {
                "كشف طبي" => AppointmentType.Medical,
                "فحص فني" => AppointmentType.Technical,
                "اختبار قيادة" => AppointmentType.Driving,
                _ => throw new AppEx.ValidationException(
                    "نوع الخدمة غير مدعوم.",
                    "INVALID_SERVICE_TYPE")
            };

        private static ServiceType MapToServiceType(string serviceType) =>
            serviceType.Trim() switch
            {
                "كشف طبي" => ServiceType.DrivingLicenseIssue,
                "فحص فني" => ServiceType.ExaminationTechnical,
                "اختبار قيادة" => ServiceType.ExaminationDriving,
                _ => throw new AppEx.ValidationException(
                    "نوع الخدمة غير مدعوم.",
                    "INVALID_SERVICE_TYPE")
            };

        private static bool IsActiveAppointmentStatus(AppointmentStatus status)
        {
            return status == AppointmentStatus.Pending || status == AppointmentStatus.Scheduled;
        }

        private async Task<int> GetLinkedApplicationIdAsync(string nationalId, ServiceType serviceType)
        {
            var serviceRequestRepo = _unitOfWork.Repository<ServiceRequest>();
            var allowedTypes = GetRelevantLinkTypes(serviceType);

            var relatedRequests = await serviceRequestRepo.FindAsync(sr =>
                sr.CitizenNationalId == nationalId &&
                sr.ReferenceId > 0 &&
                allowedTypes.Contains(sr.ServiceType));

            return relatedRequests
                .OrderByDescending(sr => sr.SubmittedAt)
                .ThenByDescending(sr => sr.LastUpdatedAt ?? sr.SubmittedAt)
                .Select(sr => sr.ReferenceId)
                .FirstOrDefault();
        }

        private static ServiceType[] GetRelevantLinkTypes(ServiceType serviceType)
        {
            return serviceType switch
            {
                ServiceType.ExaminationTechnical =>
                [
                    ServiceType.ExaminationTechnical,
                    ServiceType.VehicleLicenseIssue,
                    ServiceType.VehicleLicenseRenewal,
                    ServiceType.VehicleLicenseReplacementLost,
                    ServiceType.VehicleLicenseReplacementDamaged
                ],
                ServiceType.ExaminationDriving =>
                [
                    ServiceType.ExaminationDriving,
                    ServiceType.DrivingLicenseIssue,
                    ServiceType.DrivingLicenseRenewal,
                    ServiceType.DrivingLicenseUpgrade,
                    ServiceType.DrivingLicenseReplacementLost,
                    ServiceType.DrivingLicenseReplacementDamaged
                ],
                _ =>
                [
                    serviceType,
                    ServiceType.DrivingLicenseIssue,
                    ServiceType.DrivingLicenseRenewal,
                    ServiceType.DrivingLicenseUpgrade,
                    ServiceType.ExaminationDriving,
                    ServiceType.ExaminationTechnical
                ]
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
            TrafficUnit trafficUnit)
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
                    Notes = NullOrDefault(appointment.Notes),
                    AssignedToUserId = appointment.AssignedToUserId
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
            appointment.Notes = notes;
            appointment.AssignedToUserId = staffUserId;
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

            var governorateName = string.IsNullOrWhiteSpace(appointment.Governorate?.Name)
                ? "غير محدد"
                : appointment.Governorate!.Name;

            var trafficUnitName = string.IsNullOrWhiteSpace(appointment.TrafficUnit?.Name)
                ? "غير محدد"
                : appointment.TrafficUnit!.Name;

            return new AppointmentDto
            {
                RequestNumber = appointment.RequestNumber,
                ApplicationId = appointment.ApplicationId,
                Type = appointment.Type,
                TypeName = typeName,
                Date = appointment.Date,
                DateFormatted = FormatArabicDate(appointment.Date),
                StartTime = appointment.StartTime,
                TimeFormatted = FormatArabicTime(appointment.StartTime),
                EndTime = appointment.EndTime,
                Status = appointment.Status,
                CreatedAt = appointment.CreatedAt,
                CompletedAt = appointment.UpdatedAt,
                CitizenNationalId = appointment.CitizenNationalId,
                GovernorateId = appointment.GovernorateId,
                TrafficUnitId = appointment.TrafficUnitId,
                GovernorateName = governorateName,
                TrafficUnitName = trafficUnitName,
                Notes = appointment.Notes,
                AssignedToUserId = appointment.AssignedToUserId
            };
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

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(a =>
                    a.AssignedToUserId == null ||
                    a.AssignedToUserId == userId);
            }

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
