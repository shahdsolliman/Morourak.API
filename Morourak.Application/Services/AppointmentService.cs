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
                    "Ù„Ø§ ÙŠÙ…ÙƒÙ† Ø¹Ø±Ø¶ Ù…ÙˆØ§Ø¹ÙŠØ¯ Ù„ØªØ§Ø±ÙŠØ® Ø³Ø§Ø¨Ù‚.",
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
                    GovernorateName = "ØºÙŠØ± Ù…Ø­Ø¯Ø¯",
                    TrafficUnitName = "ØºÙŠØ± Ù…Ø­Ø¯Ø¯",
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
                    "Ù‡Ø°Ø§ Ø§Ù„Ù…ÙˆØ¹Ø¯ Ù…Ø­Ø¬ÙˆØ² Ø¨Ø§Ù„ÙØ¹Ù„ Ù„Ù‡Ø°Ù‡ Ø§Ù„Ø®Ø¯Ù…Ø©.",
                    "SLOT_UNAVAILABLE");

            var overlappingActiveAppointment = (await appointmentRepo.FindAsync(a =>
                    a.CitizenNationalId == nationalId &&
                    a.Date == request.Date &&
                    a.StartTime == request.Time))
                .Where(a => IsActiveAppointmentStatus(a.Status))
                .ToList();

            if (overlappingActiveAppointment.Any())
                throw new AppEx.ValidationException(
                    "Ù„Ø§ ÙŠÙ…ÙƒÙ† Ø­Ø¬Ø² Ø£ÙƒØ«Ø± Ù…Ù† Ù…ÙˆØ¹Ø¯ Ù†Ø´Ø· ÙÙŠ Ù†ÙØ³ Ø§Ù„ØªØ§Ø±ÙŠØ® ÙˆØ§Ù„ØªÙˆÙ‚ÙŠØª.",
                    "CITIZEN_TIME_CONFLICT");

            var serviceRequest = await FindPrimaryServiceRequestAsync(nationalId, appointmentType);
            if (serviceRequest == null)
                throw new AppEx.ValidationException(
                    "?? ??? ?????? ??? ??? ????? ???????.",
                    "?? ??? ?????? ??? ??? ????? ???????.");

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

            if (normalized.Equals("ÙƒØ´Ù Ø·Ø¨ÙŠ", StringComparison.OrdinalIgnoreCase))
                return AppointmentType.Medical;

            if (normalized.Equals("ÙØ­Øµ ÙÙ†ÙŠ", StringComparison.OrdinalIgnoreCase))
                return AppointmentType.Technical;

            if (normalized.Equals("Ø§Ø®ØªØ¨Ø§Ø± Ù‚ÙŠØ§Ø¯Ø©", StringComparison.OrdinalIgnoreCase))
                return AppointmentType.Driving;

            if (normalized.Contains("Ù…Ø±ÙƒØ¨Ø©", StringComparison.OrdinalIgnoreCase) ||
                normalized.Equals(nameof(ServiceType.VehicleLicenseIssue), StringComparison.OrdinalIgnoreCase) ||
                normalized.Equals(nameof(ServiceType.VehicleLicenseRenewal), StringComparison.OrdinalIgnoreCase) ||
                normalized.Equals(nameof(ServiceType.VehicleLicenseReplacementLost), StringComparison.OrdinalIgnoreCase) ||
                normalized.Equals(nameof(ServiceType.VehicleLicenseReplacementDamaged), StringComparison.OrdinalIgnoreCase))
            {
                return AppointmentType.Technical;
            }

            if (normalized.Contains("Ù‚ÙŠØ§Ø¯Ø©", StringComparison.OrdinalIgnoreCase) ||
                normalized.Equals(nameof(ServiceType.DrivingLicenseIssue), StringComparison.OrdinalIgnoreCase) ||
                normalized.Equals(nameof(ServiceType.DrivingLicenseRenewal), StringComparison.OrdinalIgnoreCase) ||
                normalized.Equals(nameof(ServiceType.DrivingLicenseReplacementLost), StringComparison.OrdinalIgnoreCase) ||
                normalized.Equals(nameof(ServiceType.DrivingLicenseReplacementDamaged), StringComparison.OrdinalIgnoreCase) ||
                normalized.Equals(nameof(ServiceType.DrivingLicenseUpgrade), StringComparison.OrdinalIgnoreCase))
            {
                return AppointmentType.Driving;
            }

            throw new AppEx.ValidationException(
                "Ù†ÙˆØ¹ Ø§Ù„Ø®Ø¯Ù…Ø© ØºÙŠØ± Ù…Ø¯Ø¹ÙˆÙ….",
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
                    "Ø§Ù„Ù…ÙˆØ¹Ø¯ Ø®Ø§Ø±Ø¬ Ø³Ø§Ø¹Ø§Øª Ø§Ù„Ø¹Ù…Ù„ Ø§Ù„Ø±Ø³Ù…ÙŠØ©.",
                    "INVALID_WORKING_HOURS");
        }

        private async Task<Governorate> ValidateGovernorate(int id)
        {
            var gov = await _unitOfWork.Repository<Governorate>()
                .GetByIdAsync(id);

            if (gov == null)
                throw new AppEx.ValidationException(
                    "Ø§Ù„Ù…Ø­Ø§ÙØ¸Ø© Ø§Ù„Ù…Ø®ØªØ§Ø±Ø© ØºÙŠØ± Ù…ÙˆØ¬ÙˆØ¯Ø©.",
                    "GOVERNORATE_NOT_FOUND");

            return gov;
        }

        private async Task<TrafficUnit> ValidateTrafficUnit(int govId, int unitId)
        {
            var unit = await _unitOfWork.Repository<TrafficUnit>()
                .GetAsync(u => u.Id == unitId && u.GovernorateId == govId);

            if (unit == null)
                throw new AppEx.ValidationException(
                    "ÙˆØ­Ø¯Ø© Ø§Ù„Ù…Ø±ÙˆØ± Ø§Ù„Ù…Ø®ØªØ§Ø±Ø© ØºÙŠØ± Ù…ÙˆØ¬ÙˆØ¯Ø© ÙÙŠ Ù‡Ø°Ù‡ Ø§Ù„Ù…Ø­Ø§ÙØ¸Ø©.",
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
                    Message = "ØªÙ… Ø§Ù„Ø­Ø¬Ø² Ø¨Ù†Ø¬Ø§Ø­",
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
                AppointmentType.Medical => "ÙƒØ´Ù Ø·Ø¨ÙŠ",
                AppointmentType.Driving => "Ø§Ø®ØªØ¨Ø§Ø± Ù‚ÙŠØ§Ø¯Ø©",
                AppointmentType.Technical => "ÙØ­Øµ ÙÙ†ÙŠ",
                _ => "ØºÙŠØ± Ù…Ø­Ø¯Ø¯"
            };
        }

        private static string GetServiceTypeName(ServiceType serviceType)
        {
            return serviceType switch
            {
                ServiceType.ExaminationTechnical => "ÙØ­Øµ ÙÙ†ÙŠ",
                ServiceType.ExaminationDriving => "Ø§Ø®ØªØ¨Ø§Ø± Ù‚ÙŠØ§Ø¯Ø©",
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
                PaymentStatus.Pending => "Ù‚ÙŠØ¯ Ø§Ù„Ø§Ù†ØªØ¸Ø§Ø±",
                PaymentStatus.Paid => "Ù…Ø¯ÙÙˆØ¹",
                PaymentStatus.Failed => "ÙØ´Ù„",
                _ => "ØºÙŠØ± Ù…Ø­Ø¯Ø¯"
            };
        }

        private static string NullOrDefault(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? "ØºÙŠØ± Ù…Ø­Ø¯Ø¯" : value;
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
                .Replace("AM", "ØµØ¨Ø§Ø­Ø§Ù‹")
                .Replace("PM", "Ù…Ø³Ø§Ø¡Ù‹");
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
            string requestNumber,
            AppointmentType type,
            bool passed,
            string? notes,
            string staffUserId)
        {
            if (string.IsNullOrWhiteSpace(requestNumber))
                throw new AppEx.ValidationException("??? ????? ?????.", "REQUEST_NUMBER_REQUIRED");

            if (string.IsNullOrWhiteSpace(staffUserId))
                throw new AppEx.ValidationException("????? ?????? ?????.", "STAFF_USER_REQUIRED");

            // Notes are accepted for auditing/extension, even if not persisted yet.
            _ = notes;

            var appointmentRepo = _unitOfWork.Repository<Appointment>();
            var now = DateTime.UtcNow;

            var appointment = (await appointmentRepo.FindAsync(a =>
                    a.RequestNumber == requestNumber &&
                    a.Type == type &&
                    a.Status == AppointmentStatus.Scheduled))
                .OrderByDescending(a => a.Date)
                .ThenByDescending(a => a.StartTime)
                .FirstOrDefault();

            if (appointment == null)
                throw new AppEx.ValidationException(
                    $"?? ???? ???? {type} ????? ???? ????? {requestNumber}.",
                    "APPOINTMENT_NOT_FOUND");

            var serviceRequestRepo = _unitOfWork.Repository<ServiceRequest>();
            var serviceRequest = await serviceRequestRepo.GetAsync(sr => sr.RequestNumber == requestNumber);

            if (serviceRequest == null)
                throw new AppEx.ValidationException(
                    $"??? ?????? '{requestNumber}' ??? ?????.",
                    "SERVICE_REQUEST_NOT_FOUND");

            if (serviceRequest.ReferenceId <= 0)
                throw new AppEx.ValidationException(
                    $"??? ?????? '{requestNumber}' ??? ????? ????? ????.",
                    "INVALID_SERVICE_REQUEST_REFERENCE");

            appointment.Status = passed ? AppointmentStatus.Passed : AppointmentStatus.Failed;
            appointment.UpdatedAt = now;
            appointmentRepo.Update(appointment);

            await UpdateApplicationAppointmentResultAsync(serviceRequest, type, passed);

            var requiredTypes = await GetRequiredAppointmentTypesAsync(serviceRequest);
            var requestAppointments = await appointmentRepo.FindAsync(a => a.RequestNumber == requestNumber);

            var allRequiredPassed = requiredTypes.All(requiredType =>
                requestAppointments.Any(a =>
                    a.Type == requiredType &&
                    a.Status == AppointmentStatus.Passed));

            var anyRequiredFailed = requiredTypes.Any(requiredType =>
                requestAppointments.Any(a =>
                    a.Type == requiredType &&
                    a.Status == AppointmentStatus.Failed));

            serviceRequest.Status = allRequiredPassed
                ? RequestStatus.Passed
                : anyRequiredFailed
                    ? RequestStatus.Failed
                    : RequestStatus.Pending;

            serviceRequest.LastUpdatedAt = now;
            serviceRequestRepo.Update(serviceRequest);

            await _unitOfWork.CommitAsync();
        }

        private async Task UpdateApplicationAppointmentResultAsync(
            ServiceRequest serviceRequest,
            AppointmentType appointmentType,
            bool passed)
        {
            switch (serviceRequest.ServiceType)
            {
                case ServiceType.DrivingLicenseIssue:
                    await UpdateDrivingLicenseApplicationExamFlagsAsync(serviceRequest.ReferenceId, appointmentType, passed);
                    return;

                case ServiceType.DrivingLicenseRenewal:
                case ServiceType.DrivingLicenseUpgrade:
                    await UpdateDrivingRenewalApplicationExamFlagsAsync(serviceRequest.ReferenceId, appointmentType, passed);
                    return;

                case ServiceType.VehicleLicenseIssue:
                case ServiceType.VehicleLicenseRenewal:
                    await UpdateVehicleLicenseApplicationExamFlagsAsync(serviceRequest.ReferenceId, appointmentType, passed);
                    return;

                default:
                    throw new AppEx.ValidationException(
                        $"??? ?????? '{serviceRequest.ServiceType}' ??? ????? ?????? ????????.",
                        "UNSUPPORTED_SERVICE_TYPE");
            }
        }

        private async Task UpdateDrivingLicenseApplicationExamFlagsAsync(
            int referenceId,
            AppointmentType appointmentType,
            bool passed)
        {
            var repo = _unitOfWork.Repository<DrivingLicenseApplication>();
            var application = await repo.GetByIdAsync(referenceId);

            if (application == null)
                throw new AppEx.ValidationException("??? ???? ??????? ??? ?????.", "?? ??? ?????? ??? ??? ????? ???????.");

            switch (appointmentType)
            {
                case AppointmentType.Medical:
                    application.MedicalExaminationPassed = passed;
                    break;
                case AppointmentType.Driving:
                    application.DrivingTestPassed = passed;
                    break;
                default:
                    throw new AppEx.ValidationException(
                        $"??? ?????? '{appointmentType}' ??? ???? ?????? ???? ???????.",
                        "INVALID_APPOINTMENT_TYPE");
            }

            repo.Update(application);
        }

        private async Task UpdateDrivingRenewalApplicationExamFlagsAsync(
            int referenceId,
            AppointmentType appointmentType,
            bool passed)
        {
            var repo = _unitOfWork.Repository<RenewalApplication>();
            var application = await repo.GetByIdAsync(referenceId);

            if (application == null)
                throw new AppEx.ValidationException("??? ????? ???? ??????? ??? ?????.", "?? ??? ?????? ??? ??? ????? ???????.");

            if (appointmentType == AppointmentType.Medical)
            {
                application.MedicalExaminationPassed = passed;
                repo.Update(application);
                return;
            }

            if (appointmentType != AppointmentType.Driving)
            {
                throw new AppEx.ValidationException(
                    $"??? ?????? '{appointmentType}' ??? ???? ?????? ???? ???????.",
                    "INVALID_APPOINTMENT_TYPE");
            }

            // RenewalApplication currently stores only the medical flag.
            repo.Update(application);
        }

        private async Task UpdateVehicleLicenseApplicationExamFlagsAsync(
            int referenceId,
            AppointmentType appointmentType,
            bool passed)
        {
            if (appointmentType != AppointmentType.Technical)
                throw new AppEx.ValidationException(
                    $"??? ?????? '{appointmentType}' ??? ???? ?????? ??? ????????.",
                    "INVALID_APPOINTMENT_TYPE");

            var repo = _unitOfWork.Repository<VehicleLicenseApplication>();
            var application = await repo.GetByIdAsync(referenceId);

            if (application == null)
                throw new AppEx.ValidationException("??? ???? ??????? ??? ?????.", "?? ??? ?????? ??? ??? ????? ???????.");

            application.TechnicalInspectionPassed = passed;
            repo.Update(application);
        }

        private async Task<HashSet<AppointmentType>> GetRequiredAppointmentTypesAsync(ServiceRequest serviceRequest)
        {
            switch (serviceRequest.ServiceType)
            {
                case ServiceType.DrivingLicenseIssue:
                    return [AppointmentType.Medical, AppointmentType.Driving];

                case ServiceType.DrivingLicenseRenewal:
                {
                    var renewalRepo = _unitOfWork.Repository<RenewalApplication>();
                    var renewal = await renewalRepo.GetByIdAsync(serviceRequest.ReferenceId);

                    if (renewal == null)
                        throw new AppEx.ValidationException("??? ????? ???? ??????? ??? ?????.", "?? ??? ?????? ??? ??? ????? ???????.");

                    var required = new HashSet<AppointmentType> { AppointmentType.Medical };
                    if (renewal.RequestedCategory != renewal.CurrentCategory)
                        required.Add(AppointmentType.Driving);

                    return required;
                }

                case ServiceType.DrivingLicenseUpgrade:
                    return [AppointmentType.Medical, AppointmentType.Driving];

                case ServiceType.VehicleLicenseIssue:
                case ServiceType.VehicleLicenseRenewal:
                    return [AppointmentType.Technical];

                default:
                {
                    var appointments = await _unitOfWork.Repository<Appointment>()
                        .FindAsync(a => a.RequestNumber == serviceRequest.RequestNumber);

                    return appointments
                        .Where(a => a.Status != AppointmentStatus.Cancelled)
                        .Select(a => a.Type)
                        .ToHashSet();
                }
            }
        }

        private AppointmentDto MapToDto(Appointment appointment)
        {
            var typeName = GetAppointmentTypeName(appointment.Type);
            var assignedToUserId = ResolveAssignedToUserId(appointment.Type);

            var governorateName = string.IsNullOrWhiteSpace(appointment.Governorate?.Name)
                ? "ØºÙŠØ± Ù…Ø­Ø¯Ø¯"
                : appointment.Governorate!.Name;

            var trafficUnitName = string.IsNullOrWhiteSpace(appointment.TrafficUnit?.Name)
                ? "ØºÙŠØ± Ù…Ø­Ø¯Ø¯"
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
                CompletedAt = appointment.UpdatedAt.HasValue ? FormatArabicDateTime(appointment.UpdatedAt.Value) : "ØºÙŠØ± Ù…ÙƒØªÙ…Ù„",
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
                           .Replace("AM", "ØµØ¨Ø§Ø­Ø§Ù‹")
                           .Replace("PM", "Ù…Ø³Ø§Ø¡Ù‹");
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



