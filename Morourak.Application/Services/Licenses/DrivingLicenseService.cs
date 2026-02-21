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
using Morourak.Domain.Extensions;
using System.Reflection.Emit;

namespace Morourak.Application.Services.Licenses
{
    public class DrivingLicenseService : LicenseProcessingService<DrivingLicense, DrivingLicenseApplication>, IDrivingLicenseService
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IRequestNumberGenerator _generator;

        public DrivingLicenseService(
            IUnitOfWork unitOfWork,
            IServiceRequestService serviceRequestService,
            IAppointmentService appointmentService,
            IRequestNumberGenerator generator)
            : base(unitOfWork, serviceRequestService)
        {
            _appointmentService = appointmentService;
            _generator = generator;
        }

        #region ================= Driving License First-Time Process =================


        public async Task<DrivingLicenseApplicationDto> UploadInitialDocumentsAsync(string nationalId, UploadDrivingLicenseDocumentsDto dto)
        {
            var citizen = await GetCitizenAsync(nationalId);
            if (citizen == null)
                throw new InvalidOperationException("Citizen not found");
            var existingLicense = await _licenseRepo
                    .GetAsync(l => l.CitizenRegistryId == citizen.Id);

            if (existingLicense != null)
            {
                if (existingLicense.Status == LicenseStatus.Expired)
                    throw new InvalidOperationException("Citizen has an expired license. Renewal is required.");

                if (existingLicense.Status == LicenseStatus.Active)
                    throw new InvalidOperationException("Citizen already has an active driving license.");
            }

            // Ensure no duplicate pending applications for the same citizen
            var pendingApplication = (await _unitOfWork.Repository<DrivingLicenseApplication>()
                .FindAsync(a => a.CitizenRegistryId == citizen.Id && a.Status == LicenseStatus.Pending))
                .FirstOrDefault();

            if (pendingApplication != null)
                throw new InvalidOperationException("Citizen already has a pending driving license application.");

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

            // Add and Commit (Atomic approach via UnitOfWork)
            await _unitOfWork.Repository<ServiceRequest>().AddAsync(serviceRequest);
            await _unitOfWork.CommitAsync();

            return new DrivingLicenseApplicationDto
            {
                Id = application.Id,
                Category = application.Category,
                Governorate = application.Governorate,
                LicensingUnit = application.LicensingUnit,
                Status = application.Status,
                RequestNumber = serviceRequest.RequestNumber
            };
        }

        public async Task<DrivingLicenseResponseDto> FinalizeLicenseAsync(
    string requestNumber,
    string nationalId,
    DeliveryInfoDto delivery)
        {
            // جلب المواطن
            var citizen = await GetCitizenAsync(nationalId);
            if (citizen == null)
                throw new InvalidOperationException("Citizen not found.");

            // جلب الطلب عن طريق RequestNumber
            var licenseRequest = (await _unitOfWork.Repository<ServiceRequest>()
            .FindAsync(r => r.RequestNumber == requestNumber && r.CitizenNationalId == nationalId))
            .FirstOrDefault();

            if (licenseRequest == null)
                throw new InvalidOperationException("Service request not found for this citizen.");

            // جلب application المرتبط بالـ Request
            var application = await GetApplicationForCitizenAsync(licenseRequest.ReferenceId, citizen.Id);

            // التحقق من المواعيد المطلوبة
            var appointments = await _unitOfWork.Repository<Appointment>()
                .FindAsync(a => a.ApplicationId == application.Id);

            var requiredAppointments = new AppointmentType[] { AppointmentType.Driving };

            foreach (var type in requiredAppointments)
            {
                var appointment = appointments.FirstOrDefault(a => a.Type == type);
                if (appointment == null)
                    throw new InvalidOperationException($"Appointment {type} has not been scheduled.");
                if (appointment.Status != AppointmentStatus.Passed)
                    throw new InvalidOperationException($"{type} must be passed before finalizing license.");
            }

            // التحقق من وجود رخصة سارية أو منتهية
            var existingLicense = await _licenseRepo
                .GetAsync(l => l.CitizenRegistryId == citizen.Id);

            if (existingLicense != null)
            {
                if (existingLicense.Status == LicenseStatus.Expired)
                    throw new InvalidOperationException("Citizen has an expired license. Renewal is required.");
                if (existingLicense.Status == LicenseStatus.Active)
                    throw new InvalidOperationException("Citizen already has an active driving license.");
            }

            var newLicense = await GenerateNewLicenseAsync(application, delivery);

            // ================= Persist Changes to All Related Entities =================
            // EF Core tracking is enabled (AsNoTracking removed from GenericRepo)
            // but we use explicit Update() to guarantee State is set to Modified.

            application.Status = LicenseStatus.Completed;
            _unitOfWork.Repository<DrivingLicenseApplication>().Update(application);

            licenseRequest.Status = RequestStatus.Completed;
            licenseRequest.LastUpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<ServiceRequest>().Update(licenseRequest);

            // Commit all changes in a single transaction
            await _unitOfWork.CommitAsync();

            return MapToDto(newLicense);
        }

        #endregion

        #region ================= Renew License =================

        public async Task<RenewalApplicationDto> SubmitRenewalRequestAsync(
            string nationalId,
            SubmitRenewalRequestDto dto)
        {
            var citizen = await GetCitizenAsync(nationalId);

            var license = await _licenseRepo.GetAsync(
                l => l.CitizenRegistryId == citizen.Id);


            if (license == null)
                throw new KeyNotFoundException("No driving license found for this citizen.");

            if (license.Status != LicenseStatus.Expired)
                throw new InvalidOperationException("License is still valid.");

            if (dto.MedicalCertificate == null)
                throw new InvalidOperationException("Medical certificate is required.");

            // Ensure no pending renewal for the same license
            var pendingRenewal = (await _unitOfWork.Repository<RenewalApplication>()
                .FindAsync(r => r.DrivingLicenseId == license.Id && r.Status == LicenseStatus.Pending))
                .FirstOrDefault();

            if (pendingRenewal != null)
                throw new InvalidOperationException("A renewal request is already pending for this license.");

            var medicalPath = await SaveFileAsync(dto.MedicalCertificate, "MedicalCertificates");

            var application = new RenewalApplication
            {
                CitizenRegistryId = citizen.Id,
                DrivingLicenseId = license.Id,
                CurrentCategory = license.Category,
                RequestedCategory = dto.NewCategory ?? license.Category,
                MedicalCertificatePath = medicalPath,
                Status = LicenseStatus.Pending,
                SubmittedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<RenewalApplication>().AddAsync(application);
            
            // We update the license status or simply track it if needed
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

        public async Task<DrivingLicenseResponseDto> FinalizeRenewalAsync(
    string requestNumber,
    string nationalId,
    DeliveryInfoDto delivery)
        {
            var citizen = await GetCitizenAsync(nationalId);
            if (citizen == null)
                throw new InvalidOperationException("Citizen not found.");

            var request = (await _unitOfWork.Repository<ServiceRequest>()
                .FindAsync(r =>
                    r.RequestNumber == requestNumber &&
                    r.CitizenNationalId == nationalId &&
                    r.ServiceType == ServiceType.DrivingLicenseRenewal))
                .FirstOrDefault();

            if (request == null)
                throw new InvalidOperationException("Renewal request not found.");

            var application = await _unitOfWork.Repository<RenewalApplication>()
                .GetAsync(a =>
                    a.Id == request.ReferenceId &&
                    a.CitizenRegistryId == citizen.Id);

            if (application == null)
                throw new KeyNotFoundException("Renewal application not found.");

            // Ensure Citizen is loaded for MapToDto to work correctly
            var license = await _licenseRepo.GetAsync(
                l => l.Id == application.DrivingLicenseId,
                l => l.Citizen);

            bool isUpgrade = application.RequestedCategory != application.CurrentCategory;

            if (isUpgrade)
            {
                var appointments = await _unitOfWork.Repository<Appointment>()
                    .FindAsync(a =>
                        a.ApplicationId == application.Id &&
                        a.Type == AppointmentType.Driving);

                if (!appointments.Any(a => a.Status == AppointmentStatus.Passed))
                    throw new InvalidOperationException("Driving test must be passed.");
            }

            license.Category = application.RequestedCategory;
            license.IsReplaced = false;
            license.IssueDate = DateOnly.FromDateTime(DateTime.UtcNow);
            license.ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow)
                .AddYears(GetLicenseDurationYears(license.Category));

            DeliveryFactory.ApplyDelivery(license, delivery);

            // ================= Persist Changes to All Related Entities =================
            _unitOfWork.Repository<DrivingLicense>().Update(license);

            application.Status = LicenseStatus.Completed;
            _unitOfWork.Repository<RenewalApplication>().Update(application);

            request.Status = RequestStatus.Completed;
            request.LastUpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<ServiceRequest>().Update(request);

            // Commit all changes atomically
            await _unitOfWork.CommitAsync();


            var dto = MapToDto(license);
            // Ensure Citizen navigation property was loaded via Include for name retrieval
            dto.CitizenName = $"{license.Citizen?.NameAr ?? ""} {license.Citizen?.FatherFirstNameAr ?? ""}".Trim();
            return dto;
        }

        #endregion

        #region ================= Replacement =================

        public async Task<DrivingLicenseResponseDto> IssueReplacementAsync(string nationalId,string drivingLicenseNumber,
    string replacementType,
    DeliveryInfoDto delivery)
        {
            var citizen = await GetCitizenAsync(nationalId);

            var oldLicense = await _licenseRepo.GetAsync(
                l => l.CitizenRegistryId == citizen.Id &&
                     l.LicenseNumber == drivingLicenseNumber);

            if (oldLicense == null)
                throw new KeyNotFoundException("License not found");

            ValidateReplacementEligibility(oldLicense);
            ValidateDelivery(delivery);

            // ================= Determine Service Type =================
            var normalizedType = replacementType.Trim().ToLower();

            var serviceType = normalizedType switch
            {
                "lost" => ServiceType.DrivingLicenseReplacementLost,
                "damaged" => ServiceType.DrivingLicenseReplacementDamaged,
                _ => throw new InvalidOperationException("Replacement type must be 'lost' or 'damaged'")
            };

            // ================= Update Old License =================
            oldLicense.IsReplaced = true;
            _licenseRepo.Update(oldLicense);

            // ================= Create New License =================

            var allDL = await _licenseRepo.FindAsync(l => l.LicenseNumber.StartsWith("DL"));

            var lastLicense = allDL
                .OrderByDescending(l => l.LicenseNumber)
                .FirstOrDefault();

            int nextNumber = 1;

            if (lastLicense != null)
            {
                var parts = lastLicense.LicenseNumber.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[1], out int lastNum))
                {
                    nextNumber = lastNum + 1;
                }
            }

            var newLicenseNumber = $"DL-{nextNumber}";


            var newLicense = new DrivingLicense
            {
                CitizenRegistryId = citizen.Id,
                LicenseNumber = newLicenseNumber, 
                Category = oldLicense.Category,
                Governorate = oldLicense.Governorate,
                LicensingUnit = oldLicense.LicensingUnit,
                IssueDate = DateOnly.FromDateTime(DateTime.UtcNow),
                ExpiryDate = oldLicense.ExpiryDate,
                IsReplaced = false
            };

            DeliveryFactory.ApplyDelivery(newLicense, delivery);

            await _licenseRepo.AddAsync(newLicense);
            await _unitOfWork.CommitAsync();

            // ================= Create Service Request =================
            await CreateServiceRequestAsync(
                newLicense.Id,
                serviceType,
                RequestStatus.Completed,
                nationalId
            );

            return MapToDto(newLicense);
        }

        #endregion

        #region ================= Helpers & Common Methods =================

        private async Task<byte[]> ConvertToBytesAsync(IFormFile file)
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            return ms.ToArray();
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
            var application = await _unitOfWork.Repository<DrivingLicenseApplication>()
                .GetAsync(a => a.Id == applicationId);

            if (application == null)
                throw new KeyNotFoundException("Application not found");

            if (application.CitizenRegistryId != citizenId)
                throw new UnauthorizedAccessException("Not your application");

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
                ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow)
                    .AddYears(GetLicenseDurationYears(application.Category)),
                Applications = null
            };

            DeliveryFactory.ApplyDelivery(newLicense, delivery);

            await _licenseRepo.AddAsync(newLicense);
            application.DrivingLicenseId = newLicense.Id;
            application.DrivingLicense = newLicense; // Explicit link for tracking

            await _unitOfWork.CommitAsync();

            var citizen = await _unitOfWork.Repository<CitizenRegistry>()
                .GetAsync(c => c.Id == newLicense.CitizenRegistryId);

            newLicense.Citizen = citizen;

            return newLicense;


        }

        

        #endregion

        #region ================= Validations =================

        private void ValidateDocuments(UploadDrivingLicenseDocumentsDto dto)
        {
            if (dto.PersonalPhoto == null) throw new ArgumentException("Personal photo is required.");
            if (dto.EducationalCertificate == null) throw new ArgumentException("Educational certificate is required.");
            if (dto.IdCard == null) throw new ArgumentException("ID card is required.");
            if (dto.ResidenceProof == null) throw new ArgumentException("Residence proof is required.");
            if (dto.MedicalCertificate == null) throw new ArgumentException("Medical certificate is required.");
        }

        private void ValidateDelivery(DeliveryInfoDto delivery)
        {
            if (delivery == null) throw new ArgumentException("Delivery info is required.");

            if (delivery.Method == DeliveryMethod.HomeDelivery)
            {
                if (delivery.Address == null) throw new ArgumentException("Address required for home delivery.");
                if (string.IsNullOrWhiteSpace(delivery.Address.Governorate)) throw new ArgumentException("Governorate is required.");
                if (string.IsNullOrWhiteSpace(delivery.Address.City)) throw new ArgumentException("City is required.");
                if (string.IsNullOrWhiteSpace(delivery.Address.Details)) throw new ArgumentException("Address details are required.");
            }
        }

       

        private void ValidateReplacementEligibility(DrivingLicense oldLicense)
        {
            if (oldLicense.Status != LicenseStatus.Active)
                throw new InvalidOperationException("لا يمكن إصدار بدل للرخصة منتهية أو مستبدلة.");

            if (oldLicense.ExpiryDate < DateOnly.FromDateTime(DateTime.UtcNow))
                throw new InvalidOperationException("لا يمكن إصدار بدل للرخصة منتهية الصلاحية.");
        }

        private int GetLicenseDurationYears(DrivingLicenseCategory category)
            => category == DrivingLicenseCategory.Private ? 10 : 3;

        #endregion

        #region ================= Mapping Helpers =================

        private DrivingLicenseResponseDto MapToDto(DrivingLicense license)
        {
            return new DrivingLicenseResponseDto
            {
                Id = license.Id,
                DrivingLicenseNumber = license.LicenseNumber,
                Category = license.Category.GetDisplayName(),
                Status = license.Status.GetDisplayName(),
                Governorate = license.Governorate,
                LicensingUnit = license.LicensingUnit,
                CitizenName = $"{license.Citizen?.NameAr ?? ""} {license.Citizen?.FatherFirstNameAr ?? ""}".Trim(),
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
                throw new InvalidOperationException("You already have a pending application.");

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
                MedicalCertificatePath = await SaveFileAsync(dto.MedicalCertificate, "MedicalCertificates"),
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

