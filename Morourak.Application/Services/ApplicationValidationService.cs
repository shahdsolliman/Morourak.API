using Morourak.Application.Interfaces;
using Morourak.Application.Interfaces.Services;
using Morourak.Domain.Entities;
using Morourak.Domain.Enums.Appointments;
using Morourak.Domain.Enums.Request;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Morourak.Application.Services
{
    public class ApplicationValidationService : IApplicationValidationService
    {
        private readonly IDrivingLicenseService _drivingLicenseService;
        private readonly IVehicleLicenseService _vehicleLicenseService;
        private readonly IUnitOfWork _unitOfWork;

        public ApplicationValidationService(
            IDrivingLicenseService drivingLicenseService,
            IVehicleLicenseService vehicleLicenseService,
            IUnitOfWork unitOfWork)
        {
            _drivingLicenseService = drivingLicenseService;
            _vehicleLicenseService = vehicleLicenseService;
            _unitOfWork = unitOfWork;
        }

        private static readonly Dictionary<AppointmentType, ServiceType[]> AppointmentPrerequisites = new()
        {
            { AppointmentType.Driving, Array.Empty<ServiceType>() },
            { AppointmentType.Technical, Array.Empty<ServiceType>() }
        };

        public async Task<(bool IsValid, string Message, int ApplicationId)> ValidateApplicationAsync(
            string nationalId,
            string requestNumber,
            AppointmentType type)
        {
            var serviceRequest = (await _unitOfWork.Repository<ServiceRequest>().FindAsync(
                sr => sr.RequestNumber == requestNumber &&
                      sr.CitizenNationalId == nationalId))
                .FirstOrDefault();

            if (serviceRequest == null)
                return (false, "رقم الطلب غير صحيح.", 0);

            if (serviceRequest.ReferenceId <= 0)
                return (false, "طلب الخدمة غير مرتبط بطلب رسمي.", 0);

            var applicationId = serviceRequest.ReferenceId;

            object? applicationExists =
                (object?)(await _drivingLicenseService.GetApplicationByIdAsync(applicationId, nationalId))
                ?? (await _vehicleLicenseService.GetApplicationByIdAsync(applicationId, nationalId));

            // Check RenewalApplication if not found as First-Time Application
            if (applicationExists == null && serviceRequest.ServiceType == ServiceType.DrivingLicenseRenewal)
            {
                var citizen = (await _unitOfWork.Repository<CitizenRegistry>().FindAsync(c => c.NationalId == nationalId)).FirstOrDefault();
                if (citizen != null)
                {
                    applicationExists = await _unitOfWork.Repository<RenewalApplication>()
                        .GetAsync(a => a.Id == applicationId && a.CitizenRegistryId == citizen.Id);
                }
            }

            if (applicationExists == null)
                return (false, "الطلب غير موجود لهذا المواطن.", 0);

            if (AppointmentPrerequisites.TryGetValue(type, out var requiredServices))
            {
                foreach (var service in requiredServices)
                {
                    if (!await IsServiceRequestPassedAsync(applicationId, service))
                        return (false, $"يجب اجتياز {service} أولاً.", 0);
                }
            }

            return (true, "صالح", applicationId);
        }

        private async Task<bool> IsServiceRequestPassedAsync(int applicationId, ServiceType type)
        {
            var repo = _unitOfWork.Repository<ServiceRequest>();

            var request = (await repo.GetAllAsync())
                .Where(r => r.ReferenceId == applicationId &&
                            r.ServiceType == type)
                .OrderByDescending(r => r.SubmittedAt)
                .FirstOrDefault();

            return request != null && (
                                       request.Status == RequestStatus.ReadyForProcessing ||
                                       request.Status == RequestStatus.AwaitingPayment ||
                                       request.Status == RequestStatus.Paid ||
                                       request.Status == RequestStatus.Completed);
        }
    }
}