using Morourak.Application.DTOs.Appointments;
using Morourak.Application.Interfaces;
using Morourak.Application.Interfaces.Services;
using Morourak.Domain.Entities;
using Morourak.Domain.Enums.Appointments;
using Morourak.Domain.Enums.Request;
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

        // =========================================================
        // Available Slots (Per Service - Independent)
        // =========================================================
        public async Task<IEnumerable<AppointmentDto>> GetAvailableSlotsAsync(
            DateOnly date,
            AppointmentType type,
            int trafficUnitId)
        {
            if (date < DateOnly.FromDateTime(DateTime.Today))
                throw new AppEx.ValidationException(
                    "\u0644\u0627 \u064a\u0645\u0643\u0646 \u0639\u0631\u0636 \u0645\u0648\u0627\u0639\u064a\u062f \u0644\u062a\u0627\u0631\u064a\u062e \u0633\u0627\u0628\u0642.",
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
                    TypeName = type switch
                    {
                        AppointmentType.Medical => "\u0643\u0634\u0641 \u0637\u0628\u064a",
                        AppointmentType.Driving => "\u0627\u062e\u062a\u0628\u0627\u0631 \u0642\u064a\u0627\u062f\u0629",
                        AppointmentType.Technical => "\u0641\u062d\u0635 \u0641\u0646\u064a",
                        _ => "\u063a\u064a\u0631 \u0645\u062d\u062f\u062f"
                    },
                    Date = date,
                    DateFormatted = date.ToString("d MMMM yyyy", new CultureInfo("ar-EG")),
                    StartTime = time,
                    TimeFormatted = time
                        .ToString("hh:mm tt", new CultureInfo("en-US"))
                        .Replace("AM", "\u0635\u0628\u0627\u062d\u0627\u064b")
                        .Replace("PM", "\u0645\u0633\u0627\u0621\u064b"),
                    EndTime = time.AddMinutes(SlotDurationMinutes),
                    Status = AppointmentStatus.Available,
                    GovernorateId = 0,
                    TrafficUnitId = trafficUnitId,
                    GovernorateName = "\u063a\u064a\u0631 \u0645\u062d\u062f\u062f",
                    TrafficUnitName = "\u063a\u064a\u0631 \u0645\u062d\u062f\u062f",
                    CreatedAt = DateTime.UtcNow
                });
            }

            return slots;
        }

        // =========================================================
        // Confirm Booking (Atomic - Safe - Validated)
        // =========================================================
        public async Task<BookingConfirmationDto> ConfirmBookingAsync(
            string nationalId,
            ConfirmAppointmentRequestDto request)
        {
            var appointmentType = MapToAppointmentType(request.ServiceType);
            var serviceType = MapToServiceType(request.ServiceType);

            ValidateWorkingHours(request.Time);

            await ValidateGovernorate(request.GovernorateId);
            var trafficUnit = await ValidateTrafficUnit(
                request.GovernorateId,
                request.TrafficUnitId);

            var repo = _unitOfWork.Repository<Appointment>();

            // 1) Slot taken for this service?
            var slotTaken = await repo.FindAsync(a =>
                a.TrafficUnitId == request.TrafficUnitId &&
                a.Date == request.Date &&
                a.Type == appointmentType &&
                a.StartTime == request.Time &&
                a.Status != AppointmentStatus.Cancelled);

            if (slotTaken.Any())
                throw new AppEx.ValidationException(
                    "\u0647\u0630\u0627 \u0627\u0644\u0645\u0648\u0639\u062f \u0645\u062d\u062c\u0648\u0632 \u0628\u0627\u0644\u0641\u0639\u0644 \u0644\u0647\u0630\u0647 \u0627\u0644\u062e\u062f\u0645\u0629.",
                    "SLOT_UNAVAILABLE");

            // 2) Citizen overlapping check (any service)
            var overlapping = await repo.FindAsync(a =>
                a.CitizenNationalId == nationalId &&
                a.Date == request.Date &&
                a.Status != AppointmentStatus.Cancelled &&
                request.Time < a.EndTime &&
                request.Time.AddMinutes(SlotDurationMinutes) > a.StartTime);

            if (overlapping.Any())
                throw new AppEx.ValidationException(
                    "\u0644\u0627 \u064a\u0645\u0643\u0646 \u062d\u062c\u0632 \u0623\u0643\u062b\u0631 \u0645\u0646 \u0645\u0648\u0639\u062f \u0641\u064a \u0646\u0641\u0633 \u0627\u0644\u062a\u0648\u0642\u064a\u062a.",
                    "CITIZEN_TIME_CONFLICT");

            // 3) Prevent duplicate active same type
            var duplicateType = await repo.FindAsync(a =>
                a.CitizenNationalId == nationalId &&
                a.Type == appointmentType &&
                a.Status == AppointmentStatus.Scheduled);

            if (duplicateType.Any())
                throw new AppEx.ValidationException(
                    "\u0644\u062f\u064a\u0643 \u0645\u0648\u0639\u062f \u0642\u0627\u0626\u0645 \u0628\u0627\u0644\u0641\u0639\u0644 \u0644\u0646\u0641\u0633 \u0646\u0648\u0639 \u0627\u0644\u062e\u062f\u0645\u0629.",
                    "DUPLICATE_APPOINTMENT_TYPE");

            // 4) Create service request
            var requestNumber = await _generator.GenerateAsync(serviceType);
            var serviceRequestRepo = _unitOfWork.Repository<ServiceRequest>();

            var relatedRequests = await serviceRequestRepo.FindAsync(sr =>
                sr.CitizenNationalId == nationalId &&
                sr.ReferenceId > 0 &&
                (sr.ServiceType == serviceType ||
                 sr.ServiceType == ServiceType.DrivingLicenseIssue ||
                 sr.ServiceType == ServiceType.DrivingLicenseRenewal ||
                 sr.ServiceType == ServiceType.DrivingLicenseUpgrade ||
                 sr.ServiceType == ServiceType.ExaminationDriving ||
                 sr.ServiceType == ServiceType.ExaminationTechnical));

            var applicationId = relatedRequests
                .OrderByDescending(sr => sr.SubmittedAt)
                .ThenByDescending(sr => sr.Id)
                .Select(sr => sr.ReferenceId)
                .FirstOrDefault();

            if (applicationId <= 0)
                throw new AppEx.ValidationException(
                    "No linked application found for this appointment.",
                    "APPLICATION_NOT_FOUND");
            var serviceRequest = new ServiceRequest
            {
                RequestNumber = requestNumber,
                CitizenNationalId = nationalId,
                ServiceType = serviceType,
                ReferenceId = applicationId,
                Status = RequestStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                SubmittedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            await serviceRequestRepo.AddAsync(serviceRequest);

            // 5) Create appointment
            var appointment = new Appointment
            {
                ApplicationId = applicationId,
                CitizenNationalId = nationalId,
                Date = request.Date,
                StartTime = request.Time,
                EndTime = request.Time.AddMinutes(SlotDurationMinutes),
                Status = AppointmentStatus.Scheduled,
                Type = appointmentType,
                RequestNumber = requestNumber,
                GovernorateId = request.GovernorateId,
                TrafficUnitId = request.TrafficUnitId,
                CreatedAt = DateTime.UtcNow
            };

            await repo.AddAsync(appointment);
            await _unitOfWork.CommitAsync();

            return BuildConfirmationResponse(
                request,
                trafficUnit,
                requestNumber,
                appointment.Id);
        }

        // =========================================================
        // Private Helpers
        // =========================================================
        private static AppointmentType MapToAppointmentType(string serviceType) =>
            serviceType switch
            {
                "\u0643\u0634\u0641 \u0637\u0628\u064a" => AppointmentType.Medical,
                "\u0641\u062d\u0635 \u0641\u0646\u064a" => AppointmentType.Technical,
                "\u0627\u062e\u062a\u0628\u0627\u0631 \u0642\u064a\u0627\u062f\u0629" => AppointmentType.Driving,
                _ => throw new AppEx.ValidationException(
                    "\u0646\u0648\u0639 \u0627\u0644\u062e\u062f\u0645\u0629 \u063a\u064a\u0631 \u0645\u062f\u0639\u0648\u0645.",
                    "INVALID_SERVICE_TYPE")
            };

        private static ServiceType MapToServiceType(string serviceType) =>
            serviceType switch
            {
                "\u0643\u0634\u0641 \u0637\u0628\u064a" => ServiceType.DrivingLicenseIssue,
                "\u0641\u062d\u0635 \u0641\u0646\u064a" => ServiceType.ExaminationTechnical,
                "\u0627\u062e\u062a\u0628\u0627\u0631 \u0642\u064a\u0627\u062f\u0629" => ServiceType.ExaminationDriving,
                _ => throw new AppEx.ValidationException(
                    "\u0646\u0648\u0639 \u0627\u0644\u062e\u062f\u0645\u0629 \u063a\u064a\u0631 \u0645\u062f\u0639\u0648\u0645.",
                    "INVALID_SERVICE_TYPE")
            };

        private static void ValidateWorkingHours(TimeOnly time)
        {
            if (time < WorkStart || time >= WorkEnd)
                throw new AppEx.ValidationException(
                    "\u0627\u0644\u0645\u0648\u0639\u062f \u062e\u0627\u0631\u062c \u0633\u0627\u0639\u0627\u062a \u0627\u0644\u0639\u0645\u0644 \u0627\u0644\u0631\u0633\u0645\u064a\u0629.",
                    "INVALID_WORKING_HOURS");
        }

        private async Task ValidateGovernorate(int id)
        {
            var gov = await _unitOfWork.Repository<Governorate>()
                .GetByIdAsync(id);

            if (gov == null)
                throw new AppEx.ValidationException(
                    "\u0627\u0644\u0645\u062d\u0627\u0641\u0638\u0629 \u0627\u0644\u0645\u062e\u062a\u0627\u0631\u0629 \u063a\u064a\u0631 \u0645\u0648\u062c\u0648\u062f\u0629.",
                    "GOVERNORATE_NOT_FOUND");
        }

        private async Task<TrafficUnit> ValidateTrafficUnit(int govId, int unitId)
        {
            var unit = await _unitOfWork.Repository<TrafficUnit>()
                .GetAsync(u => u.Id == unitId && u.GovernorateId == govId);

            if (unit == null)
                throw new AppEx.ValidationException(
                    "\u0648\u062d\u062f\u0629 \u0627\u0644\u0645\u0631\u0648\u0631 \u0627\u0644\u0645\u062e\u062a\u0627\u0631\u0629 \u063a\u064a\u0631 \u0645\u0648\u062c\u0648\u062f\u0629 \u0641\u064a \u0647\u0630\u0647 \u0627\u0644\u0645\u062d\u0627\u0641\u0638\u0629.",
                    "TRAFFIC_UNIT_NOT_FOUND");

            return unit;
        }

        private static BookingConfirmationDto BuildConfirmationResponse(
            ConfirmAppointmentRequestDto request,
            TrafficUnit trafficUnit,
            string requestNumber,
            int bookingId)
        {
            var culture = new CultureInfo("ar-EG");

            return new BookingConfirmationDto
            {
                Message = "\u062a\u0645 \u0627\u0644\u062d\u062c\u0632 \u0628\u0646\u062c\u0627\u062d",
                BookingNumber = bookingId.ToString(),
                RequestNumber = requestNumber,
                TrafficUnitName = trafficUnit.Name,
                TrafficUnitAddress = trafficUnit.Address ?? string.Empty,
                WorkingHours = trafficUnit.WorkingHours ?? string.Empty,
                ServiceName = request.ServiceType,
                Date = request.Date.ToString("d MMMM yyyy", culture),
                Time = request.Time
                    .ToString("hh:mm tt", new CultureInfo("en-US"))
                    .Replace("AM", "\u0635\u0628\u0627\u062d\u0627\u064b")
                    .Replace("PM", "\u0645\u0633\u0627\u0621\u064b")
            };
        }

        // ================= My Appointments =================
        public async Task<IEnumerable<AppointmentDto>> GetMyAppointmentsAsync(string nationalId)
        {
            var appointments = await _unitOfWork.Repository<Appointment>()
                .FindAsync(
                    a => a.CitizenNationalId == nationalId,
                    a => a.Governorate!,
                    a => a.TrafficUnit!);

            return appointments.Select(MapToDto).ToList();
        }

        // ================= Update Status =================
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

        // ================= Mapping Helper =================
        private AppointmentDto MapToDto(Appointment appointment)
        {
            var arCulture = new CultureInfo("ar-EG");
            var typeName = appointment.Type switch
            {
                AppointmentType.Medical => "\u0643\u0634\u0641 \u0637\u0628\u064a",
                AppointmentType.Driving => "\u0627\u062e\u062a\u0628\u0627\u0631 \u0642\u064a\u0627\u062f\u0629",
                AppointmentType.Technical => "\u0641\u062d\u0635 \u0641\u0646\u064a",
                _ => "\u063a\u064a\u0631 \u0645\u062d\u062f\u062f"
            };

            var governorateName = string.IsNullOrWhiteSpace(appointment.Governorate?.Name)
                ? "\u063a\u064a\u0631 \u0645\u062d\u062f\u062f"
                : appointment.Governorate!.Name;

            var trafficUnitName = string.IsNullOrWhiteSpace(appointment.TrafficUnit?.Name)
                ? "\u063a\u064a\u0631 \u0645\u062d\u062f\u062f"
                : appointment.TrafficUnit!.Name;

            return new AppointmentDto
            {
                RequestNumber = appointment.RequestNumber,
                ApplicationId = appointment.ApplicationId,
                Type = appointment.Type,
                TypeName = typeName,
                Date = appointment.Date,
                DateFormatted = appointment.Date.ToString("d MMMM yyyy", arCulture),
                StartTime = appointment.StartTime,
                TimeFormatted = appointment.StartTime
                    .ToString("hh:mm tt", new CultureInfo("en-US"))
                    .Replace("AM", "\u0635\u0628\u0627\u062d\u0627\u064b")
                    .Replace("PM", "\u0645\u0633\u0627\u0621\u064b"),
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

        // ================= Get Appointments By Type =================
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

        // ================= Get Appointments By Role =================
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

