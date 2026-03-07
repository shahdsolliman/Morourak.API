using Morourak.Application.DTOs.Delivery;
using Morourak.Application.DTOs.Vehicles;
using Morourak.Application.Interfaces;
using Morourak.Application.Interfaces.Services;
using Morourak.Domain.Entities;
using Morourak.Domain.Enums;
using Morourak.Domain.Enums.Common;
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

        public VehicleLicenseService(
            IUnitOfWork unitOfWork,
            IServiceRequestService serviceRequestService,
            IRequestNumberGenerator generator)
            : base(unitOfWork, serviceRequestService)
        {
            _generator = generator;
        }

        #region First-Time Process

        public async Task<VehicleLicenseApplicationDto> UploadInitialDocumentsAsync(
            string nationalId,
            UploadVehicleDocsDto dto)
        {
            var citizen = await GetCitizenAsync(nationalId);

            var existingLicense = await _licenseRepo.GetAsync(
                l => l.CitizenRegistryId == citizen.Id && l.IsReplaced == false);

            if (existingLicense != null)
            {
                if (existingLicense.Status == LicenseStatus.Active)
                    throw new AppEx.ValidationException("يمتلك المواطن رخصة سيارة سارية بالفعل.", "LICENSE_ACTIVE");
                if (existingLicense.Status == LicenseStatus.Withdrawn)
                    throw new AppEx.ValidationException("رخصة المواطن ملغاة. لا يمكن إصدار رخصة جديدة.", "LICENSE_WITHDRAWN");
            }

            var pendingApplication = (await _unitOfWork.Repository<VehicleLicenseApplication>()
                .FindAsync(a => a.CitizenRegistryId == citizen.Id && a.Status == LicenseStatus.Pending))
                .FirstOrDefault();

            if (pendingApplication != null)
                throw new AppEx.ValidationException("يوجد طلب رخصة سيارة قيد الانتظار بالفعل.", "APPLICATION_PENDING");

            ValidateDocuments(dto);

            var application = await CreateApplicationAsync(citizen.Id, dto);

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
                VehicleType = application.VehicleType.GetDisplayName(),
                Brand = application.Brand,
                Model = application.Model,
                Status = application.Status.GetDisplayName(),
                RequestNumber = serviceRequest.RequestNumber
            };
        }

        public async Task<VehicleLicenseResponseDto> FinalizeLicenseAsync(
            string requestNumber,
            string nationalId,
            DeliveryInfoDto delivery)
        {
            return await IssueVehicleLicenseAsync(requestNumber, nationalId, delivery);
        }

        #endregion

        #region Renew License

        public async Task<VehicleLicenseApplicationDto> SubmitRenewalRequestAsync(
            string nationalId,
            UploadVehicleDocsDto dto)
        {
            var citizen = await GetCitizenAsync(nationalId);

            var license = await _licenseRepo.GetAsync(
                l => l.CitizenRegistryId == citizen.Id &&
                     l.VehicleLicenseNumber == dto.VehicleLicenseNumber);

            if (license == null)
                throw new AppEx.ValidationException("رخصة السيارة غير موجودة.", "LICENSE_NOT_FOUND");

            var unpaidViolations = (await new TrafficViolationService(_unitOfWork)
                    .GetViolationsByLicenseNumberAsync(license.VehicleLicenseNumber, LicenseType.Vehicle))
                .Violations.Where(v => v.Status != "مدفوعة").ToList();

            if (unpaidViolations.Any())
                throw new AppEx.ValidationException(
                    "لا يمكن تقديم طلب تجديد لوجود مخالفات غير مدفوعة.",
                    "UNPAID_VIOLATIONS");

            if (license.Status != LicenseStatus.Expired)
                throw new AppEx.ValidationException("الرخصة لا تزال سارية. لا يلزم التجديد.", "LICENSE_STILL_VALID");

            if (license.Status == LicenseStatus.Withdrawn)
                throw new AppEx.ValidationException("لا يمكن تجديد رخصة ملغاة.", "LICENSE_WITHDRAWN");

            var pendingRenewal = (await _unitOfWork.Repository<VehicleLicenseApplication>()
                .FindAsync(a => a.CitizenRegistryId == citizen.Id &&
                                a.VehicleLicenseId == license.Id &&
                                a.Status == LicenseStatus.Pending))
                .FirstOrDefault();

            if (pendingRenewal != null)
                throw new AppEx.ValidationException("يوجد طلب تجديد قيد الانتظار بالفعل لهذه الرخصة.", "RENEWAL_PENDING");

            var application = new VehicleLicenseApplication
            {
                CitizenRegistryId = citizen.Id,
                VehicleLicenseId = license.Id,
                VehicleType = license.VehicleType,
                Brand = license.Brand,
                Model = license.Model,
                Status = LicenseStatus.Pending,
                SubmittedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<VehicleLicenseApplication>().AddAsync(application);

            license.IsPendingRenewal = true;
            _licenseRepo.Update(license);
            await _unitOfWork.CommitAsync();

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
                VehicleType = application.VehicleType.GetDisplayName(),
                Brand = application.Brand,
                Model = application.Model,
                Status = application.Status.GetDisplayName(),
                RequestNumber = serviceRequest.RequestNumber
            };
        }

        public async Task<VehicleLicenseResponseDto> FinalizeRenewalAsync(
            string requestNumber,
            string nationalId,
            DeliveryInfoDto delivery)
        {
            var citizen = await GetCitizenAsync(nationalId);

            var serviceRequest = (await _unitOfWork.Repository<ServiceRequest>()
                .FindAsync(r => r.RequestNumber == requestNumber && r.CitizenNationalId == nationalId))
                .FirstOrDefault();

            if (serviceRequest == null)
                throw new AppEx.ValidationException("طلب الخدمة غير موجود.", "REQUEST_NOT_FOUND");

            var application = await _unitOfWork.Repository<VehicleLicenseApplication>()
                .GetAsync(a => a.Id == serviceRequest.ReferenceId);

            if (application == null)
                throw new AppEx.ValidationException("طلب التجديد غير موجود.", "APPLICATION_NOT_FOUND");

            if (!application.VehicleLicenseId.HasValue)
                throw new AppEx.ValidationException("الرخصة الأصلية غير موجودة.", "LICENSE_NOT_FOUND");

            var license = await _licenseRepo.GetAsync(l => l.Id == application.VehicleLicenseId.Value);

            if (license == null)
                throw new AppEx.ValidationException("الرخصة غير موجودة.", "LICENSE_NOT_FOUND");

            ValidateDelivery(delivery);

            if (!application.TechnicalInspectionPassed)
                throw new AppEx.ValidationException(
                    "يجب اجتياز الفحص الفني قبل استكمال تجديد الرخصة.",
                    "TECHNICAL_INSPECTION_NOT_PASSED");

            // التجديد: تحديث تاريخ الاصدار والانتهاء فقط، لا يتم إنشاء رخصة جديدة
            license.IssueDate = DateTime.UtcNow;
            license.ExpiryDate = DateTime.UtcNow.AddYears(1);
            license.IsPendingRenewal = false;

            DeliveryFactory.ApplyDelivery(license, delivery);

            _licenseRepo.Update(license);

            application.Status = LicenseStatus.Completed;
            serviceRequest.Status = RequestStatus.Completed;
            serviceRequest.LastUpdatedAt = DateTime.UtcNow;

            await _unitOfWork.CommitAsync();

            return MapToDto(license);
        }

        #endregion

        #region Replacement

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
                throw new AppEx.ValidationException("رخصة السيارة غير موجودة.", "LICENSE_NOT_FOUND");

            if (oldLicense.Status == LicenseStatus.Withdrawn)
                throw new AppEx.ValidationException("لا يمكن إصدار بدل لهذه الرخصة لأنها ملغاة.", "LICENSE_WITHDRAWN");

            var unpaidViolations = (await new TrafficViolationService(_unitOfWork)
                    .GetViolationsByLicenseNumberAsync(oldLicense.VehicleLicenseNumber, LicenseType.Vehicle))
                .Violations.Where(v => v.Status != "مدفوعة").ToList();

            if (unpaidViolations.Any())
                throw new AppEx.ValidationException("توجد مخالفات غير مدفوعة على هذه الرخصة تمنع إصدار البدل.", "UNPAID_VIOLATIONS");

            ValidateReplacementEligibility(oldLicense);
            ValidateDelivery(delivery);

            var normalizedType = replacementType.Trim().ToLower();
            var serviceType = normalizedType switch
            {
                "lost" => ServiceType.VehicleLicenseReplacementLost,
                "damaged" => ServiceType.VehicleLicenseReplacementDamaged,
                _ => throw new AppEx.ValidationException("نوع البدل يجب أن يكون 'lost' أو 'damaged'.", "INVALID_REPLACEMENT_TYPE")
            };

            oldLicense.IsReplaced = true;
            _licenseRepo.Update(oldLicense);

            var newLicenseNumber = await GenerateVehicleLicenseNumberAsync();
            var plateNumber = await GeneratePlateNumberAsync();

            var newLicense = new VehicleLicense
            {
                CitizenRegistryId = citizen.Id,
                VehicleLicenseNumber = newLicenseNumber,
                PlateNumber = plateNumber,
                VehicleType = oldLicense.VehicleType,
                Brand = oldLicense.Brand,
                Model = oldLicense.Model,
                IssueDate = DateTime.UtcNow,
                ExpiryDate = oldLicense.ExpiryDate,
                IsReplaced = false
            };

            DeliveryFactory.ApplyDelivery(newLicense, delivery);

            // نقل المخالفات للرخصة الجديدة
            var violations = await _unitOfWork.Repository<TrafficViolation>()
                .FindAsync(v => v.RelatedLicenseId == oldLicense.Id);
            foreach (var v in violations)
                v.RelatedLicenseId = newLicense.Id;

            await _licenseRepo.AddAsync(newLicense);
            await _unitOfWork.CommitAsync();

            await CreateServiceRequestAsync(newLicense.Id, serviceType, RequestStatus.Completed, citizen.NationalId);

            return MapToDto(newLicense);
        }

        #endregion


        #region Queries

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
                Status = l.Status.GetDisplayName(),
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

        #region Private Helpers

        private static readonly string ArabicLetters = "أبتثجحخدذرزسشصضطظعغفقكلمنهوي";

        private string GenerateRandomPlateNumber()
        {
            var random = new Random();

            var letters = new List<string>();
            for (int i = 0; i < 3; i++)
            {
                var letter = ArabicLetters[random.Next(ArabicLetters.Length)];
                letters.Add(letter.ToString());
            }

            int number = random.Next(1000, 10000); 

            return $"{string.Join(" ", letters)} {number}";
        }

       

        private async Task<VehicleLicenseResponseDto> IssueVehicleLicenseAsync(
        string requestNumber,
        string nationalId,
        DeliveryInfoDto delivery)
        {
            var citizen = await GetCitizenAsync(nationalId);

            var serviceRequest = (await _unitOfWork.Repository<ServiceRequest>()
                .FindAsync(r => r.RequestNumber == requestNumber && r.CitizenNationalId == nationalId))
                .FirstOrDefault();

            if (serviceRequest == null)
                throw new AppEx.ValidationException("طلب الخدمة غير موجود.", "REQUEST_NOT_FOUND");

            var application = await _unitOfWork.Repository<VehicleLicenseApplication>()
                .GetAsync(a => a.Id == serviceRequest.ReferenceId);

            if (application == null)
                throw new AppEx.ValidationException("طلب الرخصة غير موجود.", "APPLICATION_NOT_FOUND");

            ValidateDelivery(delivery);

            var newLicenseNumber = await GenerateVehicleLicenseNumberAsync();
            var plateNumber = GenerateRandomPlateNumber();

            var lastLicense = (await _licenseRepo.GetAllAsync())
                .OrderByDescending(l => l.Id)
                .FirstOrDefault();

            int lastSeq = 200000;
            if (lastLicense != null && !string.IsNullOrEmpty(lastLicense.ChassisNumber))
                lastSeq = int.Parse(lastLicense.ChassisNumber.Substring(3));

            int newSeq = lastSeq + 1;

            var license = new VehicleLicense
            {
                CitizenRegistryId = citizen.Id,
                VehicleLicenseNumber = newLicenseNumber,
                PlateNumber = plateNumber,
                VehicleType = application.VehicleType,
                Brand = application.Brand,
                Model = application.Model,
                IssueDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddYears(1),
                ChassisNumber = $"CHS{newSeq:D6}",
                EngineNumber = $"ENG{newSeq:D6}",
            };

            DeliveryFactory.ApplyDelivery(license, delivery);

            await _licenseRepo.AddAsync(license);

            application.Status = LicenseStatus.Completed;
            serviceRequest.Status = RequestStatus.Completed;
            serviceRequest.LastUpdatedAt = DateTime.UtcNow;

            await _unitOfWork.CommitAsync();

            return MapToDto(license);
        }


        private async Task<VehicleLicenseApplication> CreateApplicationAsync(
            int citizenId,
            UploadVehicleDocsDto dto)
        {
            var application = new VehicleLicenseApplication
            {
                CitizenRegistryId = citizenId,
                VehicleType = dto.VehicleType,
                Brand = dto.Brand,
                Model = dto.Model,
                OwnershipProofPath = await SaveFileAsync(dto.OwnershipProof!, "OwnershipProofs"),
                VehicleDataCertificatePath = await SaveFileAsync(dto.VehicleDataCertificate!, "VehicleDataCertificates"),
                IdCardPath = await SaveFileAsync(dto.IdCard!, "IDCards"),
                InsuranceCertificatePath = await SaveFileAsync(dto.InsuranceCertificate!, "InsuranceCertificates"),
                CustomClearancePath = dto.CustomClearance != null
                    ? await SaveFileAsync(dto.CustomClearance, "CustomClearances")
                    : null,
                Status = LicenseStatus.Pending,
                SubmittedAt = DateTime.UtcNow
            };

            var repo = _unitOfWork.Repository<VehicleLicenseApplication>();
            await repo.AddAsync(application);
            await _unitOfWork.CommitAsync();

            return application;
        }

        private async Task<string> GenerateVehicleLicenseNumberAsync()
        {
            var allLicenses = await _licenseRepo.GetAllAsync();

            long maxNumber = 200000; 

            foreach (var license in allLicenses)
            {
                if (string.IsNullOrEmpty(license.VehicleLicenseNumber)) continue;

                var parts = license.VehicleLicenseNumber.Split('-');
                if (parts.Length == 2 && long.TryParse(parts[1], out var num))
                {
                    if (num > maxNumber)
                        maxNumber = num;
                }
            }

            return $"VL-{maxNumber + 1}";
        }

        private async Task<string> GeneratePlateNumberAsync()
        {
            var allLicenses = await _licenseRepo.GetAllAsync();
            var lastLicense = allLicenses
                .Where(l => !string.IsNullOrEmpty(l.PlateNumber) && l.PlateNumber.StartsWith("PL-"))
                .OrderByDescending(l => l.Id)
                .FirstOrDefault();

            long nextNumber = 10000;
            if (lastLicense != null)
            {
                var parts = lastLicense.PlateNumber.Split('-');
                if (parts.Length > 1 && long.TryParse(parts[1], out var parsed))
                    nextNumber = parsed + 1;
            }

            return $"PL-{nextNumber}";
        }

        private VehicleLicenseResponseDto MapToDto(VehicleLicense license)
        {
            return new VehicleLicenseResponseDto
            {
                Id = license.Id,
                VehicleLicenseNumber = license.VehicleLicenseNumber,
                PlateNumber = license.PlateNumber,
                VehicleType = license.VehicleType.GetDisplayName(),
                Brand = license.Brand,
                Model = license.Model,
                Status = license.Status.GetDisplayName(),
                IssueDate = DateOnly.FromDateTime(license.IssueDate),
                ExpiryDate = DateOnly.FromDateTime(license.ExpiryDate),
                CitizenNationalId = license.Citizen?.NationalId ?? "",
                CitizenName = $"{license.Citizen?.FirstName ?? ""} {license.Citizen?.LastName ?? ""}".Trim(),
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

        private void ValidateDocuments(UploadVehicleDocsDto dto)
        {
            if (dto.OwnershipProof == null)
                throw new AppEx.ValidationException("وثيقة إثبات الملكية مطلوبة.", "DOCUMENT_MISSING");
            if (dto.VehicleDataCertificate == null)
                throw new AppEx.ValidationException("شهادة بيانات المركبة مطلوبة.", "DOCUMENT_MISSING");
            if (dto.IdCard == null)
                throw new AppEx.ValidationException("بطاقة الهوية مطلوبة.", "DOCUMENT_MISSING");
            if (dto.InsuranceCertificate == null)
                throw new AppEx.ValidationException("شهادة التأمين مطلوبة.", "DOCUMENT_MISSING");
        }

        private void ValidateDelivery(DeliveryInfoDto delivery)
        {
            if (delivery == null)
                throw new AppEx.ValidationException("بيانات التسليم مطلوبة.", "DELIVERY_MISSING");
            if (delivery.Method == DeliveryMethod.HomeDelivery)
            {
                if (delivery.Address == null)
                    throw new AppEx.ValidationException("العنوان مطلوب عند اختيار التسليم المنزلي.", "ADDRESS_MISSING");
                if (string.IsNullOrWhiteSpace(delivery.Address.Governorate))
                    throw new AppEx.ValidationException("المحافظة مطلوبة.", "ADDRESS_INCOMPLETE");
                if (string.IsNullOrWhiteSpace(delivery.Address.City))
                    throw new AppEx.ValidationException("المدينة مطلوبة.", "ADDRESS_INCOMPLETE");
                if (string.IsNullOrWhiteSpace(delivery.Address.Details))
                    throw new AppEx.ValidationException("تفاصيل العنوان مطلوبة.", "ADDRESS_INCOMPLETE");
            }
        }

        private void ValidateReplacementEligibility(VehicleLicense license)
        {
            if (license.Status != LicenseStatus.Active)
                throw new AppEx.ValidationException("لا يمكن إصدار بدل لرخصة منتهية أو ملغاة أو مستبدلة بالفعل.", "LICENSE_NOT_REPLACEABLE");

            if (license.ExpiryDate < DateTime.UtcNow)
                throw new AppEx.ValidationException("لا يمكن إصدار بدل لرخصة منتهية الصلاحية.", "LICENSE_EXPIRED");
        }

        #endregion
    }
}