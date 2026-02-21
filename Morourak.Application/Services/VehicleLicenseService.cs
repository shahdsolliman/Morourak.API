//using Morourak.Application.DTOs.Delivery;
//using Morourak.Application.DTOs.Examinations;
//using Morourak.Application.DTOs.VehicleLicenses;
//using Morourak.Application.Interfaces;
//using Morourak.Application.Interfaces.Repositories;
//using Morourak.Domain.Entities;
//using Morourak.Domain.Enums;
//using Morourak.Domain.Enums.Examination;
//using Morourak.Domain.Enums.Request;

//namespace Morourak.Application.Services
//{
//    public class VehicleLicenseService : IVehicleLicenseService
//    {
//        private readonly IUnitOfWork _unitOfWork;
//        private readonly IGenericRepository<CitizenRegistry> _citizenRepo;
//        private readonly IGenericRepository<VehicleLicense> _licenseRepo;
//        private readonly IGenericRepository<ExaminationAppointment> _examRepo;
//        private readonly IServiceRequestService _serviceRequestService;

//        public VehicleLicenseService(IUnitOfWork unitOfWork, IServiceRequestService serviceRequestService)
//        {
//            _unitOfWork = unitOfWork;
//            _citizenRepo = _unitOfWork.Repository<CitizenRegistry>();
//            _licenseRepo = _unitOfWork.Repository<VehicleLicense>();
//            _examRepo = _unitOfWork.Repository<ExaminationAppointment>();
//            _serviceRequestService = serviceRequestService;

//        }

//        #region Upload Documents

//        public async Task<VehicleLicenseApplication> UploadDocumentsAsync(
//            string nationalID,
//            UploadVehicleLicenseDocumentsDto dto)
//        {
//            var citizen = await GetCitizen(nationalID);

//            if (dto.NationalID == null) throw new ArgumentException("National ID file is required.");
//            if (dto.OwnershipProof == null) throw new ArgumentException("Ownership proof file is required.");
//            if (dto.VehicleDataCertificate == null) throw new ArgumentException("Vehicle data certificate file is required.");
//            if (dto.InsuranceCertiicate == null) throw new ArgumentException("Insurance certificate file is required.");

//            var application = new VehicleLicenseApplication
//            {
//                CitizenRegistryId = citizen.Id,
//                VehicleType = dto.VehicleType,
//                Brand = dto.Brand,
//                Model = dto.Model,
//                ManufactureYear = dto.ManufactureYear,

//                IdCardPath = await SaveFileAsync(dto.NationalID, "IdCards"),
//                OwnershipProofPath = await SaveFileAsync(dto.OwnershipProof, "OwnershipProof"),
//                VehicleDataCertificatePath = await SaveFileAsync(dto.VehicleDataCertificate, "VehicleDataCertificate"),
//                InsuranceCertificatePath = await SaveFileAsync(dto.InsuranceCertiicate, "InsuranceCertificate"),

//                CustomClearancePath = dto.CustomClearance != null ? await SaveFileAsync(dto.CustomClearance, "CustomClearance") : null,
//                TechnicalInspectionReceiptPath = dto.TechnicalInspectionReceipt != null ? await SaveFileAsync(dto.TechnicalInspectionReceipt, "TechnicalInspectionReceipt") : null,

//                Status = LicenseStatus.Pending,
//                TechnicalInspectionPassed = false,
//                SubmittedAt = DateTime.UtcNow
//            };

//            var appRepo = _unitOfWork.Repository<VehicleLicenseApplication>();
//            await appRepo.AddAsync(application);

//            try
//            {
//                await _unitOfWork.CommitAsync();
//            }
//            catch
//            {
//                File.Delete(application.IdCardPath);
//                File.Delete(application.OwnershipProofPath);
//                File.Delete(application.VehicleDataCertificatePath);
//                File.Delete(application.InsuranceCertificatePath);
//                if (!string.IsNullOrEmpty(application.CustomClearancePath))
//                    File.Delete(application.CustomClearancePath);
//                if (!string.IsNullOrEmpty(application.TechnicalInspectionReceiptPath))
//                    File.Delete(application.TechnicalInspectionReceiptPath);
//                throw;
//            }

//            return application;
//        }

//        #endregion

//        #region Get All Licenses By Citizen

//        public async Task<IEnumerable<VehicleLicenseDto>> GetAllLicensesByCitizenAsync(string nationalID)
//        {
//            var citizen = await GetCitizen(nationalID);
//            var licenses = await _licenseRepo.FindAsync(
//                l => l.CitizenRegistryId == citizen.Id,
//                l => l.Examination);

//            return licenses.Select(l => new VehicleLicenseDto
//            {
//                VehicleLicenseNumber = l.VehicleLicenseNumber,
//                PlateNumber = l.PlateNumber,
//                VehicleType = l.VehicleType,
//                ExpiryDate = l.ExpiryDate,
//                Status = l.Status,
//                Brand = l.Brand,
//                Model = l.Model,
//                ManufactureYear = l.ManufactureYear,
//                CitizenName = citizen.NameAr + " " + citizen.FatherFirstNameAr,
//            });
//        }

//        #endregion

//        #region Issue New License

//        public async Task<VehicleLicenseResponseDto> IssueNewLicenseAsync(
//            string nationalID,
//            UploadVehicleLicenseDocumentsDto documentsDto,
//            VehicleLicenseDto licenseDto,
//            DeliveryInfoDto delivery)
//        {
//            await UploadDocumentsAsync(nationalID, documentsDto);

//            var citizen = await GetCitizen(nationalID);

//            var newLicense = new VehicleLicense
//            {
//                CitizenRegistryId = citizen.Id,
//                VehicleLicenseNumber = $"VL-{Guid.NewGuid().ToString()[..6]}",
//                PlateNumber = licenseDto.PlateNumber,
//                VehicleType = licenseDto.VehicleType,
//                Brand = licenseDto.Brand,
//                Model = licenseDto.Model,
//                ManufactureYear = licenseDto.ManufactureYear,
//                IssueDate = DateTime.UtcNow,
//                ExpiryDate = DateTime.UtcNow.AddYears(1),
//                Status = LicenseStatus.Pending
//            };

//            // Step 4: تطبيق Delivery
//            DeliveryFactory.ApplyDelivery(newLicense, delivery);

//            // Step 5: التحقق من آخر امتحان
//            var exam = await GetLatestExam(citizen.NationalId);
//            if (exam != null && exam.Status == ExaminationStatus.Completed)
//            {
//                newLicense.ExaminationId = exam.Id;
//                newLicense.Status = LicenseStatus.Active;
//            }

//            await _licenseRepo.AddAsync(newLicense);
//            await _unitOfWork.CommitAsync();

//            await _serviceRequestService.CreateAsync(
//            ServiceType.VehicleLicenseIssue,
//            newLicense.Id,
//            RequestStatus.InProgress
//            );

//            return MapToDto(newLicense, exam);
//        }

//        #endregion

//        #region Renew License

//        public async Task<VehicleLicenseResponseDto> RenewLicenseAsync(RenewVehicleLicenseRequestDto request)
//        {
//            var citizen = await GetCitizen(request.NationalId);
//            var license = await GetLicense(citizen.Id, request.VehicleLicenseNumber);

//            ValidateRenewalEligibility(license);

//            var exam = await GetLatestExam(citizen.NationalId);
//            if (exam == null)
//                throw new InvalidOperationException("No examination found. Please book an examination first.");

//            DeliveryFactory.ApplyDelivery(license, request.Delivery);

//            if (exam.Status == ExaminationStatus.Completed)
//            {
//                license.ExaminationId = exam.Id;
//                license.Status = LicenseStatus.Active;
//                license.IssueDate = DateTime.UtcNow;
//                license.ExpiryDate = DateTime.UtcNow.AddYears(1);
//                _licenseRepo.Update(license);
//                await _unitOfWork.CommitAsync();

//                await _serviceRequestService.CreateAsync(
//                ServiceType.VehicleLicenseRenewal,
//                license.Id,
//                license.Status == LicenseStatus.Active ? RequestStatus.Completed : RequestStatus.InProgress
//                );
//            }

//            return MapToDto(license, exam);
//        }

//        #endregion

//        #region Issue Replacement

//        public async Task<VehicleLicenseResponseDto> IssueReplacementAsync(
//            string nationalID,
//            string vehicleLicenseNumber,
//            string replacementType,
//            DeliveryInfoDto delivery)
//        {
//            var citizen = await GetCitizen(nationalID);
//            var oldLicense = await GetLicense(citizen.Id, vehicleLicenseNumber);

//            if (oldLicense.Status != LicenseStatus.Active)
//                throw new InvalidOperationException("Only active licenses can be replaced.");

//            oldLicense.Status = LicenseStatus.Replaced;
//            _licenseRepo.Update(oldLicense);

//            var lastLicense = await _licenseRepo.GetAllAsync();
//            var lastNumber = lastLicense.Any()
//                ? lastLicense.Max(l => int.Parse(l.VehicleLicenseNumber.Substring(3)))
//                : 200000;

//            var newLicenseNumber = $"VL-{lastNumber + 1}";

//            var newLicense = new VehicleLicense
//            {
//                CitizenRegistryId = citizen.Id,
//                VehicleLicenseNumber = newLicenseNumber,
//                PlateNumber = oldLicense.PlateNumber,
//                VehicleType = oldLicense.VehicleType,
//                Brand = oldLicense.Brand,
//                Model = oldLicense.Model,
//                ManufactureYear = oldLicense.ManufactureYear,
//                IssueDate = DateTime.UtcNow,
//                ExpiryDate = oldLicense.ExpiryDate,
//                Status = LicenseStatus.Pending
//            };

//            DeliveryFactory.ApplyDelivery(newLicense, delivery);

//            var exam = await GetLatestExam(citizen.NationalId);
//            if (exam != null && exam.Status == ExaminationStatus.Completed)
//            {
//                newLicense.ExaminationId = exam.Id;
//                newLicense.Status = LicenseStatus.Active;
//            }

//            await _licenseRepo.AddAsync(newLicense);
//            await _unitOfWork.CommitAsync();
//            await _serviceRequestService.CreateAsync(
//            ServiceType.VehicleLicenseReplacementDamaged,
//            newLicense.Id,
//            newLicense.Status == LicenseStatus.Active ? RequestStatus.Completed : RequestStatus.InProgress
//            );

//            return MapToDto(newLicense, exam);
//        }

//        #endregion

//        #region Helpers

//        private async Task<string> SaveFileAsync(byte[] fileBytes, string folderName)
//        {
//            var folderPath = Path.Combine("Uploads", "VehicleLicenses", folderName);
//            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

//            var fileName = $"{Guid.NewGuid()}.bin";
//            var fullPath = Path.Combine(folderPath, fileName);

//            await File.WriteAllBytesAsync(fullPath, fileBytes);
//            return fullPath;
//        }

//        private async Task<CitizenRegistry> GetCitizen(string nationalId)
//        {
//            var citizen = await _citizenRepo.GetAsync(c => c.NationalId == nationalId);
//            if (citizen == null) throw new KeyNotFoundException("Citizen not found.");
//            return citizen;
//        }

//        private async Task<VehicleLicense> GetLicense(int citizenId, string licenseNumber)
//        {
//            var license = await _licenseRepo.GetAsync(
//                l => l.CitizenRegistryId == citizenId && l.VehicleLicenseNumber == licenseNumber,
//                l => l.Examination);

//            if (license == null) throw new InvalidOperationException("License not found.");
//            return license;
//        }

//        private void ValidateRenewalEligibility(VehicleLicense license)
//        {
//            if (license.ExpiryDate > DateTime.UtcNow)
//                throw new InvalidOperationException("License is still active. Renewal not required.");
//        }

//        private async Task<ExaminationAppointment> GetLatestExam(string nationalId)
//        {
//            var exams = await _examRepo.FindAsync(e => e.CitizenNationalId == nationalId);
//            return exams.OrderByDescending(e => e.CreatedAt).FirstOrDefault();
//        }

//        private VehicleLicenseResponseDto MapToDto(
//            VehicleLicense license,
//            ExaminationAppointment exam)
//        {
//            return new VehicleLicenseResponseDto
//            {
//                VehicleLicenseNumber = license.VehicleLicenseNumber,
//                Brand = license.Brand,
//                Model = license.Model,
//                PlateNumber = license.PlateNumber,
//                IssueDate = license.IssueDate,
//                ExpiryDate = license.ExpiryDate,
//                Status = license.Status,
//                Delivery = new DeliveryInfoDto
//                {
//                    Method = license.DeliveryMethod,
//                    Address = license.DeliveryAddress == null ? null : new AddressDto
//                    {
//                        Governorate = license.DeliveryAddress.Governorate,
//                        City = license.DeliveryAddress.City,
//                        Details = license.DeliveryAddress.Details
//                    }
//                },
//                Examination = exam == null ? null : new ExaminationDto
//                {
//                    Id = exam.Id,
//                    Type = exam.Type,
//                    Date = exam.Date,
//                    StartTime = exam.StartTime,
//                    EndTime = exam.EndTime,
//                    Status = exam.Status
//                }
//            };
//        }

//        #endregion

//    }
//}