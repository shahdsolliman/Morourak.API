using Morourak.Application.DTOs.DrivingLicenses;
using Morourak.Application.Interfaces.Services;
using Morourak.Application.Interfaces;
using Morourak.Domain.Entities;
using Morourak.Domain.Enums;
using Morourak.Domain.Enums.Request;
using Morourak.Domain.Enums.Common;
using AutoMapper;
using Morourak.Application.Exceptions;
using Morourak.Application.Configurations;
using Microsoft.Extensions.Options;
using Morourak.Application.DTOs.Delivery;
using Morourak.Application.DTOs;

namespace Morourak.Application.Services.Licenses
{
    public class DrivingLicenseIssuanceService : IDrivingLicenseIssuanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILicenseValidationService _validationService;
        private readonly IRequestNumberGenerator _generator;
        private readonly IMapper _mapper;
        private readonly IServiceRequestService _serviceRequestService;
        private readonly LicenseSettings _settings;

        public DrivingLicenseIssuanceService(
            IUnitOfWork unitOfWork,
            ILicenseValidationService validationService,
            IRequestNumberGenerator generator,
            IMapper mapper,
            IServiceRequestService serviceRequestService,
            IOptions<LicenseSettings> settings)
        {
            _unitOfWork = unitOfWork;
            _validationService = validationService;
            _generator = generator;
            _mapper = mapper;
            _serviceRequestService = serviceRequestService;
            _settings = settings.Value;
        }

        public async Task<DrivingLicenseApplicationDto> UploadInitialDocumentsAsync(string nationalId, UploadDrivingLicenseDocumentsDto dto)
        {
            var citizen = await GetCitizenAsync(nationalId);
            
            _validationService.ValidateDocuments(dto);
            await _validationService.ValidateIssuanceEligibilityAsync(citizen.Id, dto.Category);

            var application = new DrivingLicenseApplication
            {
                CitizenRegistryId = citizen.Id,
                Category = dto.Category,
                Status = LicenseStatus.Pending,
                SubmittedAt = DateTime.UtcNow,
                PersonalPhotoPath = "pending", // Simplified
                EducationalCertificatePath = "pending",
                IdCardPath = "pending"
            };

            await _unitOfWork.Repository<DrivingLicenseApplication>().AddAsync(application);
            await _unitOfWork.CommitAsync(); // Get generated ID

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

            var result = _mapper.Map<DrivingLicenseApplicationDto>(application);
            result.RequestNumber = serviceRequest.RequestNumber;
            return result;
        }

        public async Task<ServiceRequestDto> FinalizeLicenseAsync(string requestNumber, string nationalId, DeliveryInfoDto delivery)
        {
            var request = await _unitOfWork.Repository<ServiceRequest>()
                .GetAsync(r => r.RequestNumber == requestNumber && r.CitizenNationalId == nationalId);
            
            if (request == null) throw new ValidationException("الطلب غير موجود.");

            var application = await _unitOfWork.Repository<DrivingLicenseApplication>()
                .GetAsync(a => a.Id == request.ReferenceId);

            if (application == null) throw new ValidationException("طلب الرخصة غير موجود.");

            if (!application.MedicalExaminationPassed)
                throw new ValidationException("لا يمكنك استكمال الطلب لعدم اجتياز الكشف الطبي.", "MEDICAL_EXAM_FAILED");

            if (!application.DrivingTestPassed)
                throw new ValidationException("لا يمكنك استكمال الطلب لعدم اجتياز اختبار القيادة.", "DRIVING_TEST_FAILED");

            _validationService.ValidateDelivery(delivery);
            var addressStr = delivery.Method == DeliveryMethod.HomeDelivery 
                ? $"{delivery.Address?.Governorate}, {delivery.Address?.City}, {delivery.Address?.Details}" 
                : null;
            
            return await _serviceRequestService.SetDeliveryAndFeesAsync(requestNumber, delivery.Method, addressStr);
        }

        public async Task<DrivingLicenseResponseDto> CompleteIssuanceAsync(string requestNumber)
        {
            var request = await _unitOfWork.Repository<ServiceRequest>().GetAsync(r => r.RequestNumber == requestNumber);
            if (request == null) throw new ValidationException("الطلب غير موجود.");

            var application = await _unitOfWork.Repository<DrivingLicenseApplication>().GetAsync(a => a.Id == request.ReferenceId);
            if (application == null) throw new ValidationException("طلب الرخصة غير موجود.");
                
            var newLicense = new DrivingLicense
            {
                CitizenRegistryId = application.CitizenRegistryId,
                LicenseNumber = $"DL-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                Category = application.Category,
                IssueDate = DateOnly.FromDateTime(DateTime.UtcNow),
                ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(_settings.GetDurationYears(application.Category))
            };

            await _unitOfWork.Repository<DrivingLicense>().AddAsync(newLicense);
            application.Status = LicenseStatus.Completed;
            await _unitOfWork.CommitAsync();

            return _mapper.Map<DrivingLicenseResponseDto>(newLicense);
        }

        private async Task<CitizenRegistry> GetCitizenAsync(string nationalId)
        {
            var citizen = await _unitOfWork.Repository<CitizenRegistry>().GetAsync(c => c.NationalId == nationalId);
            if (citizen == null) throw new ValidationException("المواطن غير موجود.", "CITIZEN_NOT_FOUND");
            return citizen;
        }
    }
}
