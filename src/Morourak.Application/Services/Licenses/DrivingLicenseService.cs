using Microsoft.AspNetCore.Http;
using Morourak.Application.DTOs.Appointments;
using Morourak.Application.DTOs.Delivery;
using Morourak.Application.DTOs.DrivingLicenses;
using Morourak.Application.DTOs.Licenses;
using Morourak.Application.Interfaces;
using Morourak.Application.Interfaces.Services;
using Morourak.Domain.Entities;
using Morourak.Domain.Enums;
using Morourak.Domain.Enums.Appointments;
using Morourak.Domain.Enums.Common;
using Morourak.Domain.Enums.Driving;
using Morourak.Domain.Enums.Request;
using Morourak.Domain.Enums.Violations;
using Morourak.Domain.Extensions;
using AppEx = Morourak.Application.Exceptions;

namespace Morourak.Application.Services.Licenses
{
    public class DrivingLicenseService : LicenseProcessingService<DrivingLicense, DrivingLicenseApplication>, IDrivingLicenseService
    {
        private readonly IRequestNumberGenerator _generator;
        private readonly ITrafficViolationService _violationService;

        public DrivingLicenseService(
            IUnitOfWork unitOfWork,
            IServiceRequestService serviceRequestService,
            IRequestNumberGenerator generator,
            ITrafficViolationService violationService)
            : base(unitOfWork, serviceRequestService)
        {
            _generator = generator;
            _violationService = violationService;
        }

        #region ================= Driving License First-Time Process =================

        public async Task<DrivingLicenseApplicationDto> UploadInitialDocumentsAsync(string nationalId, UploadDrivingLicenseDocumentsDto dto)
        {
            var citizen = await GetCitizenAsync(nationalId);
            if (citizen == null)
                throw new AppEx.ValidationException("المواطن غير موجود.", "CITIZEN_NOT_FOUND");

            var existingLicense = await _licenseRepo.GetAsync(l => l.CitizenRegistryId == citizen.Id);
            if (existingLicense != null)
            {
                if (existingLicense.Status == LicenseStatus.Expired)
                    throw new AppEx.ValidationException("للمواطن رخصة منتهية. يجب التجديد.", "LICENSE_EXPIRED");
                if (existingLicense.Status == LicenseStatus.Active)
                    throw new AppEx.ValidationException("للمواطن رخصة قيادة سارية بالفعل.", "LICENSE_ACTIVE");
                if (existingLicense.Status == LicenseStatus.Withdrawn)
                    throw new AppEx.ValidationException("للمواطن رخصة ملغاة. لا يمكن إصدار رخصة جديدة.", "LICENSE_WITHDRAWN");
            }

            var pendingApplication = (await _unitOfWork.Repository<DrivingLicenseApplication>()
                .FindAsync(a => a.CitizenRegistryId == citizen.Id && a.Status == LicenseStatus.Pending))
                .FirstOrDefault();

            if (pendingApplication != null)
                throw new AppEx.ValidationException("للمواطن طلب رخصة قيادة قيد الانتظار بالفعل.", "APPLICATION_PENDING");

            var application = await CreateApplicationAsync(nationalId, dto);

            var serviceRequest = new ServiceRequest
            {
                ReferenceId = application.Id,
                ServiceType = ServiceType.DrivingLicenseIssue,
                Status = RequestStatus.Pending,
                CitizenNationalId = citizen.NationalId,
                RequestNumber = await _generator.GenerateAsync(ServiceType.DrivingLicenseIssue),
                SubmittedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<ServiceRequest>().AddAsync(serviceRequest);
            await _unitOfWork.CommitAsync();

            return new DrivingLicenseApplicationDto
            {
                Id = application.Id,
                Category = application.Category.GetDisplayName(),
                Governorate = application.Governorate,
                LicensingUnit = application.LicensingUnit,
                Status = application.Status.GetDisplayName(),
                RequestNumber = serviceRequest.RequestNumber
            };
        }

        public async Task<Morourak.Application.DTOs.ServiceRequestDto> FinalizeLicenseAsync(
            string requestNumber,
            string nationalId,
            DeliveryInfoDto delivery)
        {
            var citizen = await GetCitizenAsync(nationalId);
            if (citizen == null)
                throw new AppEx.ValidationException("المواطن غير موجود.", "CITIZEN_NOT_FOUND");

            var licenseRequest = (await _unitOfWork.Repository<ServiceRequest>()
                .FindAsync(r => r.RequestNumber == requestNumber && r.CitizenNationalId == nationalId))
                .FirstOrDefault();

            if (licenseRequest == null)
                throw new AppEx.ValidationException("طلب الخدمة غير موجود لهذا المواطن.", "REQUEST_NOT_FOUND");

            var application = await GetApplicationForCitizenAsync(licenseRequest.ReferenceId, citizen.Id);

            if (!application.MedicalExaminationPassed)
                throw new AppEx.ValidationException("لن تتمكن من استكمال إصدار الرخصة إلا بعد اجتياز الكشف الطبي.", "MEDICAL_NOT_PASSED");

            if (!application.DrivingTestPassed)
                throw new AppEx.ValidationException("لن تتمكن من استكمال إصدار الرخصة إلا بعد اجتياز اختبار القيادة العملي.", "DRIVING_TEST_NOT_PASSED");

            ValidateDelivery(delivery);

            // Store delivery info and calculate fees
            var addressStr = delivery.Method == DeliveryMethod.HomeDelivery 
                ? $"{delivery.Address?.Governorate}, {delivery.Address?.City}, {delivery.Address?.Details}" 
                : null;

            await _unitOfWork.CommitAsync();

            return await _serviceRequestService.SetDeliveryAndFeesAsync(requestNumber, delivery.Method, addressStr);
        }

        #endregion

        #region ================= Renew License =================

        public async Task<RenewalApplicationDto> SubmitRenewalRequestAsync(
            string nationalId,
            SubmitRenewalRequestDto dto)
        {
            var citizen = await GetCitizenAsync(nationalId);
            var license = await _licenseRepo.GetAsync(l => l.CitizenRegistryId == citizen.Id);

            if (license == null)
                throw new AppEx.ValidationException("لا توجد رخصة قيادة لهذا المواطن.", "LICENSE_NOT_FOUND");
            if (license.Status != LicenseStatus.Expired)
                throw new AppEx.ValidationException("الرخصة لا تزال سارية. التجديد غير مطلوب.", "LICENSE_STILL_VALID");
            if (license.Status == LicenseStatus.Withdrawn)
                throw new AppEx.ValidationException("لا يمكن تجديد رخصة مسحوبة.", "LICENSE_WITHDRAWN");
            
            var pendingRenewal = (await _unitOfWork.Repository<RenewalApplication>()
                .FindAsync(r => r.DrivingLicenseId == license.Id && r.Status == LicenseStatus.Pending))
                .FirstOrDefault();

            if (pendingRenewal != null)
                throw new AppEx.ValidationException("يوجد طلب تجديد قيد الانتظار بالفعل لهذه الرخصة.", "RENEWAL_PENDING");

            var application = new RenewalApplication
            {
                CitizenRegistryId = citizen.Id,
                DrivingLicenseId = license.Id,
                CurrentCategory = license.Category,
                RequestedCategory = dto.NewCategory ?? license.Category,
                Status = LicenseStatus.Pending,
                SubmittedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<RenewalApplication>().AddAsync(application);
            license.IsPendingRenewal = true;
            _unitOfWork.Repository<DrivingLicense>().Update(license);
            await _unitOfWork.CommitAsync();

            var serviceRequest = new ServiceRequest
            {
                ReferenceId = application.Id,
                ServiceType = ServiceType.DrivingLicenseRenewal,
                Status = RequestStatus.Pending,
                CitizenNationalId = citizen.NationalId,
                RequestNumber = await _generator.GenerateAsync(ServiceType.DrivingLicenseRenewal),
                SubmittedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<ServiceRequest>().AddAsync(serviceRequest);
            await _unitOfWork.CommitAsync();

            return new RenewalApplicationDto
            {
                Id = application.Id,
                DrivingLicenseNumber = license.LicenseNumber,
                CurrentCategory = license.Category.GetDisplayName(),
                RequestedCategory = application.RequestedCategory.GetDisplayName(),
                Status = application.Status,
                RequestNumber = serviceRequest.RequestNumber
            };
        }

        public async Task<Morourak.Application.DTOs.ServiceRequestDto> FinalizeRenewalAsync(
            string requestNumber,
            string nationalId,
            DeliveryInfoDto delivery)
        {
            var citizen = await GetCitizenAsync(nationalId);
            if (citizen == null)
                throw new AppEx.ValidationException("المواطن غير موجود.", "CITIZEN_NOT_FOUND");

            var request = (await _unitOfWork.Repository<ServiceRequest>()
                .FindAsync(r => r.RequestNumber == requestNumber &&
                                r.CitizenNationalId == nationalId &&
                                r.ServiceType == ServiceType.DrivingLicenseRenewal))
                .FirstOrDefault();

            if (request == null)
                throw new AppEx.ValidationException("طلب التجديد غير موجود.", "REQUEST_NOT_FOUND");

            var application = await _unitOfWork.Repository<RenewalApplication>()
                .GetAsync(a => a.Id == request.ReferenceId && a.CitizenRegistryId == citizen.Id);

            if (application == null)
                throw new AppEx.ValidationException("طلب التجديد غير موجود.", "APPLICATION_NOT_FOUND");

            var license = await _licenseRepo.GetAsync(l => l.Id == application.DrivingLicenseId, l => l.Citizen);

            if (!application.MedicalExaminationPassed)
                throw new AppEx.ValidationException("يجب اجتياز الكشف الطبي قبل استكمال التجديد.", "MEDICAL_NOT_PASSED");

            if (license.Status == LicenseStatus.Withdrawn)
                throw new AppEx.ValidationException("لا يمكن تجديد رخصة مسحوبة.", "LICENSE_WITHDRAWN");

            bool isUpgrade = application.RequestedCategory != application.CurrentCategory;
            if (isUpgrade)
            {
                var appointments = await _unitOfWork.Repository<Appointment>()
                    .FindAsync(a => a.ApplicationId == application.Id && a.Type == AppointmentType.Driving);

                if (!appointments.Any(a => a.Status == AppointmentStatus.Passed))
                    throw new AppEx.ValidationException("يجب اجتياز اختبار القيادة لترقية فئة الرخصة.", "DRIVING_TEST_NOT_PASSED");
            }

            var unpaidViolations = (await _violationService
                    .GetViolationsByLicenseNumberAsync(license.LicenseNumber, LicenseType.Driving))
                .Violations.Where(v => v.Status != ViolationStatus.Paid).ToList();

            if (unpaidViolations.Any())
                throw new AppEx.ValidationException("توجد مخالفات غير مدفوعة علي هذه الرخصة تمنع استكمال تجديد الرخصة.", "UNPAID_VIOLATIONS");

            ValidateDelivery(delivery);

            var addressStr = delivery.Method == DeliveryMethod.HomeDelivery 
                ? $"{delivery.Address?.Governorate}, {delivery.Address?.City}, {delivery.Address?.Details}" 
                : null;

            request.Status = RequestStatus.Paid;
            await _unitOfWork.CommitAsync(); 

            return await _serviceRequestService.SetDeliveryAndFeesAsync(requestNumber, delivery.Method, addressStr);
        }

        #endregion

        #region ================= Replacement =================

        public async Task<Morourak.Application.DTOs.ServiceRequestDto> IssueReplacementAsync(
            string nationalId,
            string drivingLicenseNumber,
            string replacementType,
            DeliveryInfoDto delivery)
        {
            var citizen = await GetCitizenAsync(nationalId);

            var oldLicense = await _licenseRepo.GetAsync(
                l => l.CitizenRegistryId == citizen.Id &&
                     l.LicenseNumber == drivingLicenseNumber);

            if (oldLicense == null)
                throw new AppEx.ValidationException("الرخصة غير موجودة.", "LICENSE_NOT_FOUND");

            if (oldLicense.Status == LicenseStatus.Withdrawn)
                throw new AppEx.ValidationException("لا يمكن إصدار بدل لهذه الرخصة لأنها مسحوبة.", "LICENSE_WITHDRAWN");

            var unpaidViolations = (await _violationService
                    .GetViolationsByLicenseNumberAsync(oldLicense.LicenseNumber, LicenseType.Driving))
                .Violations.Where(v => v.Status != ViolationStatus.Paid).ToList();

            if (unpaidViolations.Any())
                throw new AppEx.ValidationException("توجد مخالفات غير مدفوعة علي هذه الرخصة تمنع اصدار البدل.", "UNPAID_VIOLATIONS");

            ValidateReplacementEligibility(oldLicense);
            ValidateDelivery(delivery);

            var normalizedType = replacementType.Trim().ToLower();
            var serviceType = normalizedType switch
            {
                "lost" => ServiceType.DrivingLicenseReplacementLost,
                "damaged" => ServiceType.DrivingLicenseReplacementDamaged,
                _ => throw new AppEx.ValidationException("نوع البدل يجب أن يكون 'lost' (مفقود) أو 'damaged' (تالف).", "INVALID_REPLACEMENT_TYPE")
            };

            // Create a pending service request for replacement
            var serviceRequest = new ServiceRequest
            {
                ReferenceId = oldLicense.Id, // Link to old license for reference during issuance
                ServiceType = serviceType,
                Status = RequestStatus.Pending,
                CitizenNationalId = nationalId,
                RequestNumber = await _generator.GenerateAsync(serviceType),
                SubmittedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<ServiceRequest>().AddAsync(serviceRequest);
            await _unitOfWork.CommitAsync();

            // Calculate fees and set delivery info
            var addressStr = delivery.Method == DeliveryMethod.HomeDelivery 
                ? $"{delivery.Address?.Governorate}, {delivery.Address?.City}, {delivery.Address?.Details}" 
                : null;

            return await _serviceRequestService.SetDeliveryAndFeesAsync(serviceRequest.RequestNumber, delivery.Method, addressStr);
        }

        public async Task<DrivingLicenseResponseDto> CompleteIssuanceAsync(string requestNumber)
        {
            var request = await _unitOfWork.Repository<ServiceRequest>().GetAsync(r => r.RequestNumber == requestNumber);
            if (request == null) throw new AppEx.ValidationException("الطلب غير موجود.");
            // Payment check is removed here because PaymentService already verified it and local state might not be committed yet.

            var delivery = new DeliveryInfoDto
            {
                Method = request.DeliveryMethod ?? DeliveryMethod.TrafficUnit
            };
            if (!string.IsNullOrEmpty(request.DeliveryAddressDetail))
            {
                var parts = request.DeliveryAddressDetail.Split(", ");
                delivery.Address = new AddressDto
                {
                    Governorate = parts.Length > 0 ? parts[0] : "",
                    City = parts.Length > 1 ? parts[1] : "",
                    Details = parts.Length > 2 ? parts[2] : ""
                };
            }

            DrivingLicense newLicense = null;

            if (request.ServiceType == ServiceType.DrivingLicenseIssue)
            {
                var application = await _unitOfWork.Repository<DrivingLicenseApplication>().GetAsync(a => a.Id == request.ReferenceId);
                if (application == null) throw new AppEx.ValidationException("طلب الرخصة غير موجود.");
                newLicense = await GenerateNewLicenseAsync(application, delivery);
                application.Status = LicenseStatus.Completed;
            }
            else if (request.ServiceType == ServiceType.DrivingLicenseRenewal)
            {
                var application = await _unitOfWork.Repository<RenewalApplication>().GetAsync(a => a.Id == request.ReferenceId);
                if (application == null) throw new AppEx.ValidationException("طلب التجديد غير موجود.");
                var license = await _licenseRepo.GetAsync(l => l.Id == application.DrivingLicenseId, l => l.Citizen);
                if (license == null) throw new AppEx.ValidationException("الرخصة غير موجودة.");
                
                license.Category = application.RequestedCategory;
                license.IssueDate = DateOnly.FromDateTime(DateTime.UtcNow);
                license.ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(GetLicenseDurationYears(license.Category));
                license.IsPendingRenewal = false;
                DeliveryFactory.ApplyDelivery(license, delivery);
                
                _licenseRepo.Update(license);
                application.Status = LicenseStatus.Completed;
                newLicense = license;
            }
            else if (request.ServiceType == ServiceType.DrivingLicenseReplacementLost || request.ServiceType == ServiceType.DrivingLicenseReplacementDamaged)
            {
                var oldLicense = await _licenseRepo.GetAsync(l => l.Id == request.ReferenceId);
                if (oldLicense == null) throw new AppEx.ValidationException("الرخصة الأصلية غير موجودة.");
                oldLicense.IsReplaced = true;

                var allDL = await _licenseRepo.FindAsync(l => l.LicenseNumber.StartsWith("DL"));
                var lastLicense = allDL.OrderByDescending(l => l.LicenseNumber).FirstOrDefault();
                int nextNumber = 1;
                if (lastLicense != null)
                {
                    var parts = lastLicense.LicenseNumber.Split('-');
                    if (parts.Length == 2 && int.TryParse(parts[1], out int lastNum))
                        nextNumber = lastNum + 1;
                }

                newLicense = new DrivingLicense
                {
                    CitizenRegistryId = oldLicense.CitizenRegistryId,
                    LicenseNumber = $"DL-{nextNumber}",
                    Category = oldLicense.Category,
                    Governorate = oldLicense.Governorate,
                    LicensingUnit = oldLicense.LicensingUnit,
                    IssueDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    ExpiryDate = oldLicense.ExpiryDate,
                };
                DeliveryFactory.ApplyDelivery(newLicense, delivery);
                await _licenseRepo.AddAsync(newLicense);
            }

            // request.Status = RequestStatus.Completed (Managed by PaymentService)
            // Removed internal CommitAsync to support atomic transactions in PaymentService
            // await _unitOfWork.CommitAsync(); 

            if (newLicense == null)
                throw new AppEx.ValidationException("فشل إصدار الرخصة.", "ISSUANCE_FAILED");

            return MapToDto(newLicense);
        }

        #endregion

        #region ================= Helpers & Common Methods =================

        public async Task SubmitAppointmentResultAsync(int applicationId, AppointmentType type, bool passed, string? notes)
        {
            var repo = _unitOfWork.Repository<Appointment>();
            var appointment = (await repo.FindAsync(a => a.ApplicationId == applicationId && a.Type == type))
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefault();

            if (appointment == null)
                throw new AppEx.ValidationException($"موعد {type} غير موجود.", "APPOINTMENT_NOT_FOUND");

            appointment.Status = passed ? AppointmentStatus.Passed : AppointmentStatus.Failed;
            repo.Update(appointment);

            var applicationRepo = _unitOfWork.Repository<DrivingLicenseApplication>();
            var application = await applicationRepo.GetAsync(a => a.Id == applicationId);

            if (application == null)
                throw new AppEx.ValidationException("الطلب غير موجود.", "APPLICATION_NOT_FOUND");

            if (type == AppointmentType.Medical)
                application.MedicalExaminationPassed = passed;
            if (type == AppointmentType.Driving)
                application.DrivingTestPassed = passed;

            applicationRepo.Update(application);
            await _unitOfWork.CommitAsync();
        }


        public async Task<DrivingLicenseApplication> GetApplicationByIdAsync(int applicationId, string nationalId)
        {
            var citizen = await GetCitizenAsync(nationalId);
            var application = await _unitOfWork.Repository<DrivingLicenseApplication>()
                .GetAsync(a => a.Id == applicationId && a.CitizenRegistryId == citizen.Id);
            return application;
        }

        private async Task<DrivingLicenseApplication> GetApplicationForCitizenAsync(int applicationId, int citizenId)
        {
            var application = await _unitOfWork.Repository<DrivingLicenseApplication>().GetAsync(a => a.Id == applicationId);
            if (application == null)
                throw new AppEx.ValidationException("الطلب غير موجود.", "APPLICATION_NOT_FOUND");
            if (application.CitizenRegistryId != citizenId)
                throw new AppEx.ValidationException("ليس لديك صلاحية للوصول إلى هذا الطلب.", "AUTHZ_ERROR");
            return application;
        }

        private async Task<DrivingLicense> GenerateNewLicenseAsync(
            DrivingLicenseApplication application,
            DeliveryInfoDto delivery)
        {
            var lastLicense = (await _licenseRepo.GetAllAsync())
                .Where(l => l.LicenseNumber.StartsWith("DL-"))
                .OrderByDescending(l => l.Id)
                .FirstOrDefault();

            long nextNumber = 100000;
            if (lastLicense != null)
            {
                var parts = lastLicense.LicenseNumber.Split('-');
                if (parts.Length > 1 && long.TryParse(parts[1], out var parsed))
                    nextNumber = parsed + 1;
            }

            var newLicense = new DrivingLicense
            {
                CitizenRegistryId = application.CitizenRegistryId,
                LicenseNumber = $"DL-{nextNumber}",
                Category = application.Category,
                Governorate = application.Governorate,
                LicensingUnit = application.LicensingUnit,
                IssueDate = DateOnly.FromDateTime(DateTime.UtcNow),
                ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(GetLicenseDurationYears(application.Category)),
                Applications = null
            };

            DeliveryFactory.ApplyDelivery(newLicense, delivery);

            await _licenseRepo.AddAsync(newLicense);
            application.DrivingLicenseId = newLicense.Id;
            application.DrivingLicense = newLicense;

            // Removed internal CommitAsync to support atomic transactions in PaymentService
            // await _unitOfWork.CommitAsync();

            var citizen = await _unitOfWork.Repository<CitizenRegistry>()
                .GetAsync(c => c.Id == newLicense.CitizenRegistryId);
            newLicense.Citizen = citizen;

            return newLicense;
        }

        #endregion

        #region ================= Validations =================

        private void ValidateDocuments(UploadDrivingLicenseDocumentsDto dto)
        {
            if (dto.PersonalPhoto == null) throw new AppEx.ValidationException("الصورة الشخصية مطلوبة.", "DOCUMENT_MISSING");
            if (dto.EducationalCertificate == null) throw new AppEx.ValidationException("الشهادة الدراسية مطلوبة.", "DOCUMENT_MISSING");
            if (dto.IdCard == null) throw new AppEx.ValidationException("بطاقة الرقم القومي مطلوبة.", "DOCUMENT_MISSING");
            if (dto.ResidenceProof == null) throw new AppEx.ValidationException("إثبات محل الإقامة مطلوب.", "DOCUMENT_MISSING");
        }

        private void ValidateDelivery(DeliveryInfoDto delivery)
        {
            if (delivery == null) throw new AppEx.ValidationException("معلومات التوصيل مطلوبة.", "DELIVERY_MISSING");
            if (delivery.Method == DeliveryMethod.HomeDelivery)
            {
                if (delivery.Address == null) throw new AppEx.ValidationException("العنوان مطلوب للتوصيل للمنزل.", "ADDRESS_MISSING");
                if (string.IsNullOrWhiteSpace(delivery.Address.Governorate)) throw new AppEx.ValidationException("المحافظة مطلوبة.", "ADDRESS_INCOMPLETE");
                if (string.IsNullOrWhiteSpace(delivery.Address.City)) throw new AppEx.ValidationException("المدينة مطلوبة.", "ADDRESS_INCOMPLETE");
                if (string.IsNullOrWhiteSpace(delivery.Address.Details)) throw new AppEx.ValidationException("تفاصيل العنوان مطلوبة.", "ADDRESS_INCOMPLETE");
            }
        }

        private void ValidateReplacementEligibility(DrivingLicense oldLicense)
        {
            if (oldLicense.Status != LicenseStatus.Active)
                throw new AppEx.ValidationException("لا يمكن إصدار بدل لرخصة منتهية أو مسحوبة أو تم استبدالها بالفعل.", "LICENSE_NOT_REPLACEABLE");

            if (oldLicense.ExpiryDate < DateOnly.FromDateTime(DateTime.UtcNow))
                throw new AppEx.ValidationException("لا يمكن إصدار بدل لرخصة منتهية.", "LICENSE_EXPIRED");
        }

        private int GetLicenseDurationYears(DrivingLicenseCategory category)
            => category == DrivingLicenseCategory.Private ? 10 : 3;

        #endregion

        #region ================= Mapping Helpers =================

        private DrivingLicenseResponseDto MapToDto(DrivingLicense? license)
        {
            if (license == null) return new DrivingLicenseResponseDto();

            return new DrivingLicenseResponseDto
            {
                Id = license.Id,
                DrivingLicenseNumber = license.LicenseNumber,
                Category = license.Category.GetDisplayName(),
                Status = license.Status.GetDisplayName(),
                Governorate = license.Governorate,
                LicensingUnit = license.LicensingUnit,
                CitizenName = $"{license.Citizen?.FirstName ?? ""} {license.Citizen?.LastName ?? ""}".Trim(),
                IssueDate = license.IssueDate,
                ExpiryDate = license.ExpiryDate,
                Delivery = new DeliveryInfoDto
                {
                    Method = license.DeliveryMethod,
                    Address = license.DeliveryAddress == null ? null : new AddressDto
                    {
                        Governorate = license.DeliveryAddress.Governorate,
                        City = license.DeliveryAddress.City,
                        Details = license.DeliveryAddress.Details
                    }
                }
            };
        }

        #endregion

        #region ================= Application Creation =================

        private async Task<DrivingLicenseApplication> CreateApplicationAsync(
            string nationalId,
            UploadDrivingLicenseDocumentsDto dto)
        {
            var citizen = await GetCitizenAsync(nationalId);
            ValidateDocuments(dto);

            var repo = _unitOfWork.Repository<DrivingLicenseApplication>();

            var existingApplication = (await repo.FindAsync(a =>
                a.CitizenRegistryId == citizen.Id &&
                a.Status == LicenseStatus.Pending &&
                a.DrivingLicenseId == null &&
                a.Category == dto.Category &&
                a.Governorate == dto.Governorate &&
                a.LicensingUnit == dto.LicensingUnit
            )).FirstOrDefault();

            if (existingApplication != null)
                throw new AppEx.ValidationException("لديك طلب قيد الانتظار بالفعل.", "APPLICATION_PENDING");

            var application = new DrivingLicenseApplication
            {
                CitizenRegistryId = citizen.Id,
                Category = dto.Category,
                Governorate = dto.Governorate,
                LicensingUnit = dto.LicensingUnit,
                PersonalPhotoPath = await SaveFileAsync(dto.PersonalPhoto, "PersonalPhotos"),
                EducationalCertificatePath = await SaveFileAsync(dto.EducationalCertificate, "EducationalCertificates"),
                IdCardPath = await SaveFileAsync(dto.IdCard, "IDCards"),
                ResidenceProofPath = await SaveFileAsync(dto.ResidenceProof, "ResidenceProof"),
                Status = LicenseStatus.Pending,
                SubmittedAt = DateTime.UtcNow
            };

            await repo.AddAsync(application);
            await _unitOfWork.CommitAsync();

            return application;
        }

        #endregion

        public async Task<IEnumerable<DrivingLicenseDto>> GetAllLicensesByCitizenAsync(string nationalId)
        {
            var citizen = await GetCitizenAsync(nationalId);
            var licenses = await _licenseRepo.FindAsync(l => l.CitizenRegistryId == citizen.Id);

            return licenses.Select(l => new DrivingLicenseDto
            {
                LicenseNumber = l.LicenseNumber,
                Category = l.Category.GetDisplayName(),
                Governorate = l.Governorate,
                LicensingUnit = l.LicensingUnit,
                Status = l.Status.GetDisplayName(),
                IssueDate = l.IssueDate,
                ExpiryDate = l.ExpiryDate,
                CitizenNationalId = citizen.NationalId,
            });
        }
    }
}