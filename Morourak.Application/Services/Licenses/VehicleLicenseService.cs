using Morourak.Application.DTOs.Delivery;
using Morourak.Application.DTOs.Vehicles;
using Morourak.Application.Interfaces;
using Morourak.Application.Interfaces.Services;
using Morourak.Domain.Entities;
using Morourak.Domain.Enums;
using Morourak.Domain.Enums.Appointments;
using Morourak.Domain.Enums.Request;
using Morourak.Domain.Enums.Vehicles;
using Morourak.Domain.Extensions;

namespace Morourak.Application.Services.Licenses
{
    public class VehicleLicenseService : LicenseProcessingService<VehicleLicense, VehicleLicenseApplication>, IVehicleLicenseService
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

        public async Task<VehicleLicenseApplicationDto> UploadInitialDocumentsAsync(string nationalId, UploadVehicleDocsDto dto)
        {
            var citizen = await GetCitizenAsync(nationalId);

            var repo = _unitOfWork.Repository<VehicleLicenseApplication>();
            var pendingApplication = (await repo.FindAsync(a => a.CitizenRegistryId == citizen.Id && a.Status == LicenseStatus.Pending)).FirstOrDefault();

            if (pendingApplication != null)
                throw new InvalidOperationException("Citizen already has a pending vehicle license application.");

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
                CustomClearancePath = dto.CustomClearance != null ? await SaveFileAsync(dto.CustomClearance, "CustomClearances") : null,
                Status = LicenseStatus.Pending,
                SubmittedAt = DateTime.UtcNow
            };

            await repo.AddAsync(application);
            await _unitOfWork.CommitAsync();

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

        public async Task<VehicleLicenseApplicationDto> RenewLicenseAsync(string nationalId, UploadVehicleDocsDto dto)
        {
            var citizen = await GetCitizenAsync(nationalId);

            var license = await _licenseRepo.GetAsync(
                l => l.CitizenRegistryId == citizen.Id && 
                     l.VehicleLicenseNumber == dto.VehicleLicenseNumber);

            if (license == null)
                throw new KeyNotFoundException("Active vehicle license not found for renewal.");

            if (license.Status != LicenseStatus.Active && license.Status != LicenseStatus.Expired)
                throw new InvalidOperationException("This license is not eligible for renewal.");

            // Check for unpaid violations
            var unpaidViolations = await _unitOfWork.Repository<VehicleViolation>().FindAsync(v => v.VehicleLicenseId == license.Id && !v.IsPaid);
            if (unpaidViolations.Any())
            {
                throw new InvalidOperationException("يجب سداد جميع المخالفات المرورية قبل إتمام عملية التجديد. برجاء التوجه لخطوة السداد أولاً.");
            }

            var repo = _unitOfWork.Repository<VehicleLicenseApplication>();
            var pendingRenewal = (await repo.FindAsync(a => 
                a.CitizenRegistryId == citizen.Id && 
                a.VehicleLicenseId == license.Id &&
                a.Status == LicenseStatus.Pending)).FirstOrDefault();

            if (pendingRenewal != null)
                throw new InvalidOperationException("A renewal application is already pending for this vehicle.");

            var application = new VehicleLicenseApplication
            {
                CitizenRegistryId = citizen.Id,
                VehicleLicenseId = license.Id,
                VehicleType = license.VehicleType,
                Brand = license.Brand,
                Model = license.Model,
                ManufactureYear = license.ManufactureYear,
                Governorate = dto.Governorate,
                OwnershipProofPath = license.PlateNumber, // Keep as ref if needed or set null
                Status = LicenseStatus.Pending,
                SubmittedAt = DateTime.UtcNow
            };

            // Only save files if they are provided (for first-time issuance or optional renewal docs)
            if (dto.VehicleDataCertificate != null)
                application.VehicleDataCertificatePath = await SaveFileAsync(dto.VehicleDataCertificate, "VehicleDataCertificates");
            
            if (dto.IdCard != null)
                application.IdCardPath = await SaveFileAsync(dto.IdCard, "IDCards");

            if (dto.InsuranceCertificate != null)
                application.InsuranceCertificatePath = await SaveFileAsync(dto.InsuranceCertificate, "InsuranceCertificates");

            if (dto.OwnershipProof != null)
                application.OwnershipProofPath = await SaveFileAsync(dto.OwnershipProof, "OwnershipProofs");

            await repo.AddAsync(application);
            
            // Mark license as pending renewal
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
                VehicleType = application.VehicleType,
                Brand = application.Brand,
                Model = application.Model,
                ManufactureYear = application.ManufactureYear,
                Governorate = application.Governorate,
                Status = application.Status,
                RequestNumber = serviceRequest.RequestNumber
            };
        }

        public async Task<VehicleLicenseResponseDto> FinalizeRenewalAsync(string requestNumber, string nationalId, DeliveryInfoDto delivery)
        {
             var citizen = await GetCitizenAsync(nationalId);
            
            var serviceRequest = (await _unitOfWork.Repository<ServiceRequest>()
                .FindAsync(r => r.RequestNumber == requestNumber && r.CitizenNationalId == nationalId && r.ServiceType == ServiceType.VehicleLicenseRenewal))
                .FirstOrDefault();

            if (serviceRequest == null)
                throw new InvalidOperationException("Renewal service request not found.");

            var application = await _unitOfWork.Repository<VehicleLicenseApplication>().GetAsync(a => a.Id == serviceRequest.ReferenceId);
            if (application == null) throw new KeyNotFoundException("Application not found.");

            var license = await _licenseRepo.GetAsync(l => l.Id == application.VehicleLicenseId);
            if (license == null) throw new KeyNotFoundException("License not found for renewal.");

            // Final check for unpaid violations before completing renewal
            var unpaidViolations = await _unitOfWork.Repository<VehicleViolation>().FindAsync(v => v.VehicleLicenseId == license.Id && !v.IsPaid);
            if (unpaidViolations.Any())
            {
                throw new InvalidOperationException("يجب سداد جميع المخالفات المرورية قبل إتمام عملية التجديد.");
            }

            // Check Technical Inspection
            var appointments = await _examRepo.FindAsync(a => a.ApplicationId == application.Id);
            var techInspection = appointments.FirstOrDefault(a => a.Type == AppointmentType.Technical);

            if (techInspection == null)
                throw new InvalidOperationException("Technical inspection appointment has not been scheduled for renewal.");
            if (techInspection.Status != AppointmentStatus.Passed)
                throw new InvalidOperationException("Technical inspection must be passed before finalizing renewal.");

            // Update License
            license.IssueDate = DateTime.UtcNow;
            license.ExpiryDate = DateTime.UtcNow.AddYears(1); // Renew for 1 year
            license.IsPendingRenewal = false;
            license.Governorate = application.Governorate;
            license.DeliveryMethod = delivery.Method;

            DeliveryFactory.ApplyDelivery(license, delivery);

            _licenseRepo.Update(license);
            
            application.Status = LicenseStatus.Completed;
            serviceRequest.Status = RequestStatus.Completed;
            serviceRequest.LastUpdatedAt = DateTime.UtcNow;
            
            await _unitOfWork.CommitAsync();

            return await MapToDtoAsync(license);
        }

        public async Task<VehicleLicenseResponseDto> FinalizeLicenseAsync(string requestNumber, string nationalId, DeliveryInfoDto delivery)
        {
            var citizen = await GetCitizenAsync(nationalId);
            
            var serviceRequest = (await _unitOfWork.Repository<ServiceRequest>()
                .FindAsync(r => r.RequestNumber == requestNumber && r.CitizenNationalId == nationalId))
                .FirstOrDefault();

            if (serviceRequest == null)
                throw new InvalidOperationException("Service request not found.");

            var application = await _unitOfWork.Repository<VehicleLicenseApplication>().GetAsync(a => a.Id == serviceRequest.ReferenceId);
            if (application == null) throw new KeyNotFoundException("Application not found.");

            // Check Technical Inspection
            var appointments = await _examRepo.FindAsync(a => a.ApplicationId == application.Id);
            var techInspection = appointments.FirstOrDefault(a => a.Type == AppointmentType.Technical);

            if (techInspection == null)
                throw new InvalidOperationException("Technical inspection appointment has not been scheduled.");
            if (techInspection.Status != AppointmentStatus.Passed)
                throw new InvalidOperationException("Technical inspection must be passed before finalizing license.");

            // Generate License
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
                ChassisNumber = "CH-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
                EngineNumber = "EN-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
                DeliveryMethod = delivery.Method,
                Citizen = citizen // Link for tracking
            };

            DeliveryFactory.ApplyDelivery(newLicense, delivery);

            await _licenseRepo.AddAsync(newLicense);
            
            application.Status = LicenseStatus.Completed;
            serviceRequest.Status = RequestStatus.Completed;
            
            await _unitOfWork.CommitAsync();

            return await MapToDtoAsync(newLicense);
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
                VehicleType = l.VehicleType.ToString(),
                Brand = l.Brand,
                Model = l.Model,
                ManufactureYear = l.ManufactureYear,
                Status = l.Status.GetDisplayName(),
                Governorate = l.Governorate,
                IssueDate = l.IssueDate,
                ExpiryDate = l.ExpiryDate,
                CitizenNationalId = nationalId
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

        public async Task<IEnumerable<VehicleViolationDto>> GetVehicleViolationsAsync(int vehicleLicenseId)
        {
            var violations = await _unitOfWork.Repository<VehicleViolation>().FindAsync(v => v.VehicleLicenseId == vehicleLicenseId);
            return violations.Select(v => new VehicleViolationDto
            {
                Id = v.Id,
                ViolationDate = v.ViolationDate,
                ViolationType = v.ViolationType,
                Amount = v.Amount,
                Description = v.Description,
                IsPaid = v.IsPaid
            });
        }

        public async Task<VehicleLicenseApplication?> GetApplicationByIdAsync(int id, string nationalId)
        {
            var citizen = await GetCitizenAsync(nationalId);
            return await _unitOfWork.Repository<VehicleLicenseApplication>().GetAsync(a => a.Id == id && a.CitizenRegistryId == citizen.Id);
        }

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
                throw new KeyNotFoundException("Vehicle license not found");

            if (oldLicense.Status != LicenseStatus.Active)
                throw new InvalidOperationException("Only active vehicle licenses can be replaced.");

            // Determine Service Type
            var normalizedType = replacementType.Trim().ToLower();
            var serviceType = normalizedType switch
            {
                "lost" => ServiceType.VehicleLicenseReplacementLost,
                "damaged" => ServiceType.VehicleLicenseReplacementDamaged,
                _ => throw new InvalidOperationException("Replacement type must be 'lost' or 'damaged'")
            };

            // Update Old License
            oldLicense.IsReplaced = true;
            _licenseRepo.Update(oldLicense);

            // Create New License
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
                ManufactureYear = oldLicense.ManufactureYear,
                Governorate = oldLicense.Governorate,
                IssueDate = DateTime.UtcNow,
                ExpiryDate = oldLicense.ExpiryDate,
                ChassisNumber = oldLicense.ChassisNumber,
                EngineNumber = oldLicense.EngineNumber,
                DeliveryMethod = delivery.Method,
                Citizen = citizen
            };

            DeliveryFactory.ApplyDelivery(newLicense, delivery);

            await _licenseRepo.AddAsync(newLicense);
            await _unitOfWork.CommitAsync();

            // Create Service Request
            await CreateServiceRequestAsync(
                newLicense.Id,
                serviceType,
                RequestStatus.Completed,
                nationalId
            );

            return await MapToDtoAsync(newLicense);
        }

        #region Helpers

        private async Task<string> GenerateVehicleLicenseNumberAsync()
        {
             var lastLicenseList = (await _licenseRepo.GetAllAsync());
             var lastLicense = lastLicenseList
                .OrderByDescending(l => l.Id)
                .FirstOrDefault();

            long nextNumber = 200001;
            if (lastLicense != null && !string.IsNullOrEmpty(lastLicense.VehicleLicenseNumber))
            {
                var parts = lastLicense.VehicleLicenseNumber.Split('-');
                if (parts.Length > 1 && long.TryParse(parts.Last(), out var parsed))
                {
                    nextNumber = parsed + 1;
                }
            }
            return $"VL-{nextNumber}";
        }

        private async Task<string> GeneratePlateNumberAsync()
        {
            // Just for demonstration, making it async-compliant for future expansions
            return await Task.FromResult("ABC-" + new Random().Next(1000, 9999));
        }

        private async Task<VehicleLicenseResponseDto> MapToDtoAsync(VehicleLicense license)
        {
            var citizen = license.Citizen ?? await _citizenRepo.GetAsync(c => c.Id == license.CitizenRegistryId);
            var violations = await GetVehicleViolationsAsync(license.Id);

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
                CitizenName = citizen != null ? $"{citizen.NameAr} {citizen.FatherFirstNameAr}".Trim() : "",
                Violations = violations.ToList()
            };
        }

        #endregion
    }
}