using Morourak.Application.DTOs.Delivery;
using Morourak.Application.DTOs.Vehicles;
using Morourak.Application.Interfaces;
using Morourak.Application.Interfaces.Services;
using Morourak.Domain.Entities;
using Morourak.Domain.Enums;
using Morourak.Domain.Enums.Appointments;
using Morourak.Domain.Enums.Request;
using Morourak.Domain.Enums.Vehicles;
using Morourak.Domain.Enums.Violations;
using Morourak.Domain.Extensions;
using AppEx = Morourak.Application.Exceptions;

namespace Morourak.Application.Services.Licenses
{
    public class VehicleLicenseService
        : LicenseProcessingService<VehicleLicense, VehicleLicenseApplication>,
          IVehicleLicenseService
    {
        private readonly IRequestNumberGenerator _generator;
        private readonly IUnitOfWork _unitOfWork;

        public VehicleLicenseService(
            IUnitOfWork unitOfWork,
            IServiceRequestService serviceRequestService,
            IRequestNumberGenerator generator)
            : base(unitOfWork, serviceRequestService)
        {
            _unitOfWork = unitOfWork;
            _generator = generator;
        }

        #region ================= License Issuance =================

        public async Task<VehicleLicenseApplicationDto> UploadInitialDocumentsAsync(
            string nationalId,
            UploadVehicleDocsDto dto)
        {
            var citizen = await GetCitizenAsync(nationalId);

            var repo = _unitOfWork.Repository<VehicleLicenseApplication>();

            // تحقق من وجود طلب معلق لنفس المواطن
            var pendingApplication = (await repo.FindAsync(a =>
                a.CitizenRegistryId == citizen.Id &&
                a.Status == LicenseStatus.Pending)).FirstOrDefault();

            if (pendingApplication != null)
                throw new AppEx.ValidationException(
                    "Citizen already has a pending vehicle license application.",
                    "APPLICATION_PENDING");

            // إنشاء التطبيق الجديد
            var application = new VehicleLicenseApplication
            {
                CitizenRegistryId = citizen.Id,
                VehicleType = dto.VehicleType,
                Brand = dto.Brand,
                Model = dto.Model,
                ManufactureYear = dto.ManufactureYear,
                Governorate = dto.Governorate,
                OwnershipProofPath = await SaveFileAsync(dto.OwnershipProof, "OwnershipProofs"),
                VehicleDataCertificatePath = await SaveFileAsync(dto.VehicleDataCertificate, "VehicleDataCertificates"),
                IdCardPath = await SaveFileAsync(dto.IdCard, "IDCards"),
                InsuranceCertificatePath = await SaveFileAsync(dto.InsuranceCertificate, "InsuranceCertificates"),
                CustomClearancePath = dto.CustomClearance != null
                    ? await SaveFileAsync(dto.CustomClearance, "CustomClearances")
                    : null,
                Status = LicenseStatus.Pending,
                SubmittedAt = DateTime.UtcNow
            };

            await repo.AddAsync(application);
            await _unitOfWork.CommitAsync();

            // إنشاء طلب الخدمة
            var serviceRequest = new ServiceRequest
            {
                ReferenceId = application.Id,
                ServiceType = ServiceType.VehicleLicenseIssue,
                Status = RequestStatus.Pending,
                CitizenNationalId = citizen.NationalId,
                RequestNumber = await _generator.GenerateAsync(ServiceType.VehicleLicenseIssue),
                SubmittedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<ServiceRequest>().AddAsync(serviceRequest);
            await _unitOfWork.CommitAsync();

            return new VehicleLicenseApplicationDto
            {
                Id = application.Id,
                VehicleType = application.VehicleType,
                Brand = application.Brand,
                Model = application.Model,
                ManufactureYear = application.ManufactureYear,
                Governorate = application.Governorate,
                Status = application.Status,
                RequestNumber = serviceRequest.RequestNumber
            };
        }

        #endregion

        #region ================= License Renewal =================

        public async Task<VehicleLicenseApplicationDto> RenewLicenseAsync(
            string nationalId,
            UploadVehicleDocsDto dto)
        {
            var citizen = await GetCitizenAsync(nationalId);

            var license = await _licenseRepo.GetAsync(
                l => l.CitizenRegistryId == citizen.Id &&
                     l.VehicleLicenseNumber == dto.VehicleLicenseNumber);

            if (license == null)
                throw new AppEx.ValidationException(
                    "Active vehicle license not found for renewal.",
                    "LICENSE_NOT_FOUND");

            if (license.Status != LicenseStatus.Active &&
                license.Status != LicenseStatus.Expired)
                throw new AppEx.ValidationException(
                    "This license is not eligible for renewal.",
                    "LICENSE_NOT_ELIGIBLE");

            var repo = _unitOfWork.Repository<VehicleLicenseApplication>();

            var pendingRenewal = (await repo.FindAsync(a =>
                a.CitizenRegistryId == citizen.Id &&
                a.VehicleLicenseId == license.Id &&
                a.Status == LicenseStatus.Pending)).FirstOrDefault();

            if (pendingRenewal != null)
                throw new AppEx.ValidationException(
                    "There is already a pending renewal request for this vehicle.",
                    "RENEWAL_PENDING");

            // إنشاء طلب التجديد
            var application = new VehicleLicenseApplication
            {
                CitizenRegistryId = citizen.Id,
                VehicleLicenseId = license.Id,
                VehicleType = license.VehicleType,
                Brand = license.Brand,
                Model = license.Model,
                ManufactureYear = license.ManufactureYear,
                Governorate = dto.Governorate,
                Status = LicenseStatus.Pending,
                SubmittedAt = DateTime.UtcNow
            };

            await repo.AddAsync(application);

            // تحديث حالة الرخصة بأنها في تجديد
            license.IsPendingRenewal = true;
            _licenseRepo.Update(license);
            await _unitOfWork.CommitAsync();

            // إنشاء طلب الخدمة
            var serviceRequest = new ServiceRequest
            {
                ReferenceId = application.Id,
                ServiceType = ServiceType.VehicleLicenseRenewal,
                Status = RequestStatus.Pending,
                CitizenNationalId = citizen.NationalId,
                RequestNumber = await _generator.GenerateAsync(ServiceType.VehicleLicenseRenewal),
                SubmittedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<ServiceRequest>().AddAsync(serviceRequest);
            await _unitOfWork.CommitAsync();

            return new VehicleLicenseApplicationDto
            {
                Id = application.Id,
                VehicleType = application.VehicleType,
                Brand = application.Brand,
                Model = application.Model,
                ManufactureYear = application.ManufactureYear,
                Governorate = application.Governorate,
                Status = application.Status,
                RequestNumber = serviceRequest.RequestNumber
            };
        }

        #endregion

        #region ================= Finalize License / Renewal =================

        public async Task<VehicleLicenseResponseDto> FinalizeLicenseAsync(
            string requestNumber,
            string nationalId,
            DeliveryInfoDto delivery)
        {
            return await FinalizeVehicleLicenseInternal(requestNumber, nationalId, delivery, isRenewal: false);
        }

        public async Task<VehicleLicenseResponseDto> FinalizeRenewalAsync(
            string requestNumber,
            string nationalId,
            DeliveryInfoDto delivery)
        {
            return await FinalizeVehicleLicenseInternal(requestNumber, nationalId, delivery, isRenewal: true);
        }

        private async Task<VehicleLicenseResponseDto> FinalizeVehicleLicenseInternal(
            string requestNumber,
            string nationalId,
            DeliveryInfoDto delivery,
            bool isRenewal)
        {
            var citizen = await GetCitizenAsync(nationalId);

            var serviceRequest = (await _unitOfWork.Repository<ServiceRequest>()
                .FindAsync(r => r.RequestNumber == requestNumber && r.CitizenNationalId == nationalId))
                .FirstOrDefault();

            if (serviceRequest == null)
                throw new AppEx.ValidationException(
                    "Service request not found.",
                    "REQUEST_NOT_FOUND");

            var application = await _unitOfWork.Repository<VehicleLicenseApplication>()
                .GetAsync(a => a.Id == serviceRequest.ReferenceId);
            if (application == null)
                throw new AppEx.ValidationException(
                    "Vehicle license application not found.",
                    "APPLICATION_NOT_FOUND");

            // التحقق من المخالفات
            var violationService = new TrafficViolationService(_unitOfWork);
            var licenseNumber = application.VehicleLicenseId != null
                ? (await _licenseRepo.GetAsync(l => l.Id == application.VehicleLicenseId.Value)).VehicleLicenseNumber
                : "";

            var unpaidViolations = (await violationService.GetViolationsByLicenseNumberAsync(
                    licenseNumber, LicenseType.Vehicle))
                .Violations
                .Where(v => v.Status != "مدفوعة")
                .ToList();

            if (unpaidViolations.Any())
                throw new AppEx.ValidationException(
                    $"All traffic violations must be paid before finalizing {(isRenewal ? "renewal" : "issuance")} of license. Unpaid violations: {unpaidViolations.Count}",
                    "UNPAID_VIOLATIONS");

            // التحقق من الفحص الفني
            var appointments = await _examRepo.FindAsync(a => a.ApplicationId == application.Id);
            var techInspection = appointments.FirstOrDefault(a => a.Type == AppointmentType.Technical);

            if (techInspection == null)
                throw new AppEx.ValidationException(
                    "Technical inspection not scheduled.",
                    "INSPECTION_NOT_SCHEDULED");
            if (techInspection.Status != AppointmentStatus.Passed)
                throw new AppEx.ValidationException(
                    "Technical inspection must be passed before finalizing license.",
                    "INSPECTION_NOT_PASSED");

            var newLicenseNumber = await GenerateVehicleLicenseNumberAsync();
            var plateNumber = await GeneratePlateNumberAsync();

            var newLicense = new VehicleLicense
            {
                CitizenRegistryId = citizen.Id,
                VehicleLicenseNumber = newLicenseNumber,
                PlateNumber = plateNumber,
                VehicleType = application.VehicleType,
                Brand = application.Brand,
                Model = application.Model,
                ManufactureYear = application.ManufactureYear,
                Governorate = application.Governorate,
                IssueDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddYears(1),
                DeliveryMethod = delivery.Method
            };

            DeliveryFactory.ApplyDelivery(newLicense, delivery);
            await _licenseRepo.AddAsync(newLicense);

            application.Status = LicenseStatus.Completed;
            serviceRequest.Status = RequestStatus.Completed;

            await _unitOfWork.CommitAsync();

            return await MapToDtoAsync(newLicense);
        }

        #endregion

        #region ================= Replacement =================

        public async Task<VehicleLicenseResponseDto> IssueReplacementAsync(
            string nationalId,
            string vehicleLicenseNumber,
            string replacementType,
            DeliveryInfoDto delivery)
        {
            var citizen = await GetCitizenAsync(nationalId);

            var oldLicense = await _licenseRepo.GetAsync(
                l => l.CitizenRegistryId == citizen.Id &&
                     l.VehicleLicenseNumber == vehicleLicenseNumber);

            if (oldLicense == null)
                throw new AppEx.ValidationException("Vehicle license not found.", "LICENSE_NOT_FOUND");

            if (oldLicense.Status != LicenseStatus.Active)
                throw new AppEx.ValidationException("Only active licenses can be replaced.", "LICENSE_NOT_ELIGIBLE");

            var normalizedType = replacementType.Trim().ToLower();
            var serviceType = normalizedType switch
            {
                "lost" => ServiceType.VehicleLicenseReplacementLost,
                "damaged" => ServiceType.VehicleLicenseReplacementDamaged,
                _ => throw new AppEx.ValidationException(
                    "Replacement type must be 'lost' or 'damaged'.",
                    "INVALID_REPLACEMENT_TYPE")
            };

            oldLicense.IsReplaced = true;
            _licenseRepo.Update(oldLicense);

            var allVL = await _licenseRepo.FindAsync(l => l.VehicleLicenseNumber.StartsWith("VL"));
            var lastLicense = allVL
                .OrderByDescending(l => l.VehicleLicenseNumber)
                .FirstOrDefault();

            long nextNumber = 200001;
            if (lastLicense != null)
            {
                var parts = lastLicense.VehicleLicenseNumber.Split('-');
                if (parts.Length == 2 && long.TryParse(parts[1], out long lastNum))
                    nextNumber = lastNum + 1;
            }

            var newLicense = new VehicleLicense
            {
                CitizenRegistryId = citizen.Id,
                VehicleLicenseNumber = $"VL-{nextNumber}",
                VehicleType = oldLicense.VehicleType,
                Brand = oldLicense.Brand,
                Model = oldLicense.Model,
                ManufactureYear = oldLicense.ManufactureYear,
                Governorate = oldLicense.Governorate,
                IssueDate = DateTime.UtcNow,
                ExpiryDate = oldLicense.ExpiryDate,
                DeliveryMethod = delivery.Method,
                IsReplaced = false
            };

            DeliveryFactory.ApplyDelivery(newLicense, delivery);

            var violations = await _unitOfWork.Repository<TrafficViolation>()
                .FindAsync(v => v.RelatedLicenseId == oldLicense.Id);
            foreach (var v in violations)
                v.RelatedLicenseId = newLicense.Id;

            await _licenseRepo.AddAsync(newLicense);
            await _unitOfWork.CommitAsync();

            var serviceRequest = new ServiceRequest
            {
                ReferenceId = newLicense.Id,
                ServiceType = serviceType,
                Status = RequestStatus.Completed,
                CitizenNationalId = citizen.NationalId,
                RequestNumber = await _generator.GenerateAsync(serviceType),
                SubmittedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<ServiceRequest>().AddAsync(serviceRequest);
            await _unitOfWork.CommitAsync();

            return await MapToDtoAsync(newLicense);
        }

        #endregion

        #region ================= Helpers =================

        private async Task<string> GenerateVehicleLicenseNumberAsync()
        {
            var lastLicense = (await _licenseRepo.GetAllAsync())
                .OrderByDescending(l => l.Id)
                .FirstOrDefault();

            long nextNumber = 200001;
            if (lastLicense != null && !string.IsNullOrEmpty(lastLicense.VehicleLicenseNumber))
            {
                var parts = lastLicense.VehicleLicenseNumber.Split('-');
                if (parts.Length > 1 && long.TryParse(parts.Last(), out var parsed))
                    nextNumber = parsed + 1;
            }

            return $"VL-{nextNumber}";
        }

        private async Task<string> GeneratePlateNumberAsync()
        {
            return await Task.FromResult("ABC-" + new Random().Next(1000, 9999));
        }

        private async Task<VehicleLicenseResponseDto> MapToDtoAsync(VehicleLicense license)
        {
            var citizen = license.Citizen ?? await _citizenRepo.GetAsync(c => c.Id == license.CitizenRegistryId);

            return new VehicleLicenseResponseDto
            {
                Id = license.Id,
                VehicleLicenseNumber = license.VehicleLicenseNumber,
                PlateNumber = license.PlateNumber,
                VehicleType = license.VehicleType.GetDisplayName(),
                Brand = license.Brand,
                Model = license.Model,
                ManufactureYear = license.ManufactureYear,
                Status = license.Status.GetDisplayName(),
                Governorate = license.Governorate,
                IssueDate = license.IssueDate,
                ExpiryDate = license.ExpiryDate,
                CitizenNationalId = citizen?.NationalId ?? "",
                CitizenName = citizen != null ? $"{citizen.FirstName} {citizen.LastName}".Trim() : ""
            };
        }

        #endregion

        #region Interface Implementations

        public async Task<VehicleLicenseApplication?> GetApplicationByIdAsync(int id, string nationalId)
        {
            var citizen = await GetCitizenAsync(nationalId);

            return await _unitOfWork.Repository<VehicleLicenseApplication>()
                .GetAsync(a => a.Id == id && a.CitizenRegistryId == citizen.Id);
        }

        public async Task<IEnumerable<VehicleLicenseDto>> GetAllLicensesByCitizenAsync(string nationalId)
        {
            var citizen = await GetCitizenAsync(nationalId);

            var licenses = await _licenseRepo.FindAsync(l => l.CitizenRegistryId == citizen.Id);

            return licenses.Select(l => new VehicleLicenseDto
            {
                Id = l.Id,
                VehicleLicenseNumber = l.VehicleLicenseNumber,
                PlateNumber = l.PlateNumber,
                VehicleType = l.VehicleType.GetDisplayName(),
                Brand = l.Brand,
                Model = l.Model,
                ManufactureYear = l.ManufactureYear,
                Status = l.Status.GetDisplayName(),
                Governorate = l.Governorate,
                IssueDate = l.IssueDate,
                ExpiryDate = l.ExpiryDate,
                CitizenNationalId = citizen.NationalId
            });
        }

        public async Task<IEnumerable<VehicleTypeDetailDto>> GetVehicleTypesAsync()
        {
            var entities = await _unitOfWork.Repository<VehicleTypeEntity>().GetAllAsync();

            return entities.GroupBy(e => e.VehicleType)
                .Select(typeGroup => new VehicleTypeDetailDto
                {
                    Value = Enum.TryParse<VehicleType>(typeGroup.Key, out var vType) ? (int)vType : 0,
                    Name = typeGroup.Key,
                    NameAr = typeGroup.First().VehicleTypeAr,
                    Brands = typeGroup.GroupBy(e => e.Brand)
                        .Select(brandGroup => new BrandDetailDto
                        {
                            Name = brandGroup.Key,
                            NameAr = brandGroup.First().BrandAr,
                            Models = brandGroup.Select(e => new ModelDetailDto
                            {
                                Name = e.Model,
                                NameAr = e.ModelAr
                            }).ToList()
                        }).ToList()
                });
        }

        #endregion
    }
}