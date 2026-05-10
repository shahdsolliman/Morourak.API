using Morourak.Application.Interfaces;
using Morourak.Application.Interfaces.Services;
using Morourak.Domain.Entities;
using Morourak.Domain.Enums.Appointments;
using Morourak.Domain.Enums.Request;
using Morourak.Domain.Extensions;
using AppEx = Morourak.Application.Exceptions;

namespace Morourak.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AppointmentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task UpdateStatusAsync(
            string requestNumber,
            AppointmentType type,
            bool passed,
            string? notes,
            string staffUserId)
        {
            if (string.IsNullOrWhiteSpace(requestNumber))
                throw new AppEx.ValidationException("رقم الطلب مطلوب.", "REQUEST_NUMBER_REQUIRED");

            if (string.IsNullOrWhiteSpace(staffUserId))
                throw new AppEx.ValidationException("معرف الموظف مطلوب.", "STAFF_USER_REQUIRED");

            // Notes are accepted for auditing/extension, even if not persisted yet.
            _ = notes;

            var appointmentRepo = _unitOfWork.Repository<Morourak.Domain.Entities.Appointment>();
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
                    $"لا يوجد موعد {type} مجدول لرقم الطلب {requestNumber}.",
                    "APPOINTMENT_NOT_FOUND");

            var serviceRequestRepo = _unitOfWork.Repository<ServiceRequest>();
            var serviceRequest = await serviceRequestRepo.GetAsync(sr => sr.RequestNumber == requestNumber);

            if (serviceRequest == null)
                throw new AppEx.ValidationException(
                    $"طلب الخدمة '{requestNumber}' غير موجود.",
                    "SERVICE_REQUEST_NOT_FOUND");

            if (serviceRequest.ReferenceId <= 0)
                throw new AppEx.ValidationException(
                    $"طلب الخدمة '{requestNumber}' لا يحتوي على مرجع صالح.",
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

            // Only advance to Passed if we are in the appointments/validation phase.
            // If already AwaitingPayment or Paid, do not regress to Passed.
            if (serviceRequest.Status == RequestStatus.Pending || 
                serviceRequest.Status == RequestStatus.InProgress)
            {
                serviceRequest.Status = allRequiredPassed
                    ? RequestStatus.ReadyForProcessing
                    : anyRequiredFailed
                        ? RequestStatus.Failed
                        : RequestStatus.Pending;
            }

            serviceRequest.LastUpdatedAt = now;
            serviceRequestRepo.Update(serviceRequest);

            await _unitOfWork.CommitAsync();
        }

        public async Task UpdateStatusAsync(int id, AppointmentStatus status, string? notes, string staffUserId)
        {
            var appointmentRepo = _unitOfWork.Repository<Morourak.Domain.Entities.Appointment>();
            var appointment = await appointmentRepo.GetByIdAsync(id);

            if (appointment == null)
                throw new AppEx.ValidationException("الموعد غير موجود.", "APPOINTMENT_NOT_FOUND");

            if (string.IsNullOrEmpty(appointment.StaffId))
            {
                appointment.StaffId = staffUserId;
                // Note: In a real system, we might fetch the staff name from IdentityService here.
                // For now, we just assign the ID.
            }

            appointment.Status = status;
            appointment.UpdatedAt = DateTime.UtcNow;
            
            appointmentRepo.Update(appointment);

            // If status is Passed or Failed, we might need to update the related ServiceRequest
            // We can reuse the logic from the other UpdateStatusAsync if needed
            if (status == AppointmentStatus.Passed || status == AppointmentStatus.Failed)
            {
                var passed = status == AppointmentStatus.Passed;
                var serviceRequestRepo = _unitOfWork.Repository<ServiceRequest>();
                var serviceRequest = await serviceRequestRepo.GetAsync(sr => sr.RequestNumber == appointment.RequestNumber);
                
                if (serviceRequest != null)
                {
                    await UpdateApplicationAppointmentResultAsync(serviceRequest, appointment.Type, passed);
                    
                    var requiredTypes = await GetRequiredAppointmentTypesAsync(serviceRequest);
                    var requestAppointments = await appointmentRepo.FindAsync(a => a.RequestNumber == appointment.RequestNumber);

                    var allRequiredPassed = requiredTypes.All(requiredType =>
                        requestAppointments.Any(a =>
                            a.Type == requiredType &&
                            a.Status == AppointmentStatus.Passed));

                    var anyRequiredFailed = requiredTypes.Any(requiredType =>
                        requestAppointments.Any(a =>
                            a.Type == requiredType &&
                            a.Status == AppointmentStatus.Failed));

                    if (serviceRequest.Status == RequestStatus.Pending || 
                        serviceRequest.Status == RequestStatus.InProgress)
                    {
                        serviceRequest.Status = allRequiredPassed
                            ? RequestStatus.ReadyForProcessing
                            : anyRequiredFailed
                                ? RequestStatus.Failed
                                : RequestStatus.Pending;
                    }
                    serviceRequest.LastUpdatedAt = DateTime.UtcNow;
                    serviceRequestRepo.Update(serviceRequest);
                }
            }

            await _unitOfWork.CommitAsync();
        }

        public async Task AssignStaffAsync(int appointmentId, string staffId, string staffName)
        {
            var appointmentRepo = _unitOfWork.Repository<Morourak.Domain.Entities.Appointment>();
            var appointment = await appointmentRepo.GetByIdAsync(appointmentId);

            if (appointment == null)
                throw new AppEx.ValidationException("الموعد غير موجود.", "APPOINTMENT_NOT_FOUND");

            appointment.StaffId = staffId;
            appointment.StaffName = staffName;
            appointment.UpdatedAt = DateTime.UtcNow;

            appointmentRepo.Update(appointment);
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
                        $"نوع الخدمة '{serviceRequest.ServiceType}' غير مدعوم لتحديث النتائج.",
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
                throw new AppEx.ValidationException("طلب رخصة القيادة غير موجود.", "APPLICATION_NOT_FOUND");

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
                        $"نوع الموعد '{appointmentType}' غير صالح لطلب رخصة القيادة.",
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
                throw new AppEx.ValidationException("طلب تجديد الرخصة غير موجود.", "APPLICATION_NOT_FOUND");

            if (appointmentType == AppointmentType.Medical)
            {
                application.MedicalExaminationPassed = passed;
                repo.Update(application);
                return;
            }

            if (appointmentType != AppointmentType.Driving)
            {
                throw new AppEx.ValidationException(
                    $"نوع الموعد '{appointmentType}' غير صالح لطلب التجديد.",
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
                    $"نوع الموعد '{appointmentType}' غير صالح لفحص المركبة.",
                    "INVALID_APPOINTMENT_TYPE");

            var repo = _unitOfWork.Repository<VehicleLicenseApplication>();
            var application = await repo.GetByIdAsync(referenceId);

            if (application == null)
                throw new AppEx.ValidationException("طلب رخصة المركبة غير موجود.", "APPLICATION_NOT_FOUND");

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
                        throw new AppEx.ValidationException("طلب تجديد الرخصة غير موجود.", "APPLICATION_NOT_FOUND");

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
                    var appointments = await _unitOfWork.Repository<Morourak.Domain.Entities.Appointment>()
                        .FindAsync(a => a.RequestNumber == serviceRequest.RequestNumber);

                    return appointments
                        .Where(a => a.Status != AppointmentStatus.Cancelled)
                        .Select(a => a.Type)
                        .ToHashSet();
                }
            }
        }


    }
}
