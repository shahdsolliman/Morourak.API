using Morourak.Application.DTOs.DrivingLicenses;
using Morourak.Application.Interfaces.Services;
using Morourak.Application.Interfaces;
using Morourak.Domain.Entities;
using Morourak.Domain.Enums;
using Morourak.Domain.Enums.Request;
using Morourak.Domain.Enums.Common;
using AutoMapper;
using Morourak.Application.Exceptions;
using Morourak.Application.Interfaces.Repositories;
using Morourak.Application.Configurations;
using Microsoft.Extensions.Options;
using Morourak.Application.DTOs.Delivery;
using Morourak.Application.DTOs;

namespace Morourak.Application.Services.Licenses
{
    public class DrivingLicenseRenewalService : IDrivingLicenseRenewalService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILicenseValidationService _validationService;
        private readonly IRequestNumberGenerator _generator;
        private readonly IMapper _mapper;
        private readonly IServiceRequestService _serviceRequestService;

        public DrivingLicenseRenewalService(
            IUnitOfWork unitOfWork,
            ILicenseValidationService validationService,
            IRequestNumberGenerator generator,
            IMapper mapper,
            IServiceRequestService serviceRequestService)
        {
            _unitOfWork = unitOfWork;
            _validationService = validationService;
            _generator = generator;
            _mapper = mapper;
            _serviceRequestService = serviceRequestService;
        }

        public async Task<RenewalApplicationDto> SubmitRenewalRequestAsync(string nationalId, SubmitRenewalRequestDto dto)
        {
            var citizen = await GetCitizenAsync(nationalId);
            
            var license = await _unitOfWork.Repository<DrivingLicense>().GetAsync(l => 
                l.CitizenRegistryId == citizen.Id && 
                l.LicenseNumber == dto.LicenseNumber);

            if (license == null)
                throw new ValidationException("الرخصة غير موجودة أو لا تخص هذا المواطن.", "LICENSE_NOT_FOUND");

            await _validationService.ValidateRenewalEligibilityAsync(license, dto.NewCategory ?? license.Category);

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

            // Commit here to get the generated application.Id
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

            var result = _mapper.Map<RenewalApplicationDto>(application);
            result.DrivingLicenseNumber = license.LicenseNumber;
            result.RequestNumber = serviceRequest.RequestNumber;
            return result;
        }

        public async Task<ServiceRequestDto> FinalizeRenewalAsync(string requestNumber, string nationalId, DeliveryInfoDto delivery)
        {
            var request = await _unitOfWork.Repository<ServiceRequest>()
                .GetAsync(r => r.RequestNumber == requestNumber && r.CitizenNationalId == nationalId);

            if (request == null) throw new ValidationException("الطلب غير موجود.");

            var application = await _unitOfWork.Repository<RenewalApplication>()
                .GetAsync(a => a.Id == request.ReferenceId);

            if (application == null) throw new ValidationException("طلب الرخصة غير موجود.");

            if (!application.MedicalExaminationPassed)
                throw new ValidationException("لا يمكنك استكمال الطلب لعدم اجتياز الكشف الطبي.", "MEDICAL_EXAM_FAILED");

            _validationService.ValidateDelivery(delivery);
            var addressStr = delivery.Method == DeliveryMethod.HomeDelivery 
                ? $"{delivery.Address?.Governorate}, {delivery.Address?.City}, {delivery.Address?.Details}" 
                : null;
            
            return await _serviceRequestService.SetDeliveryAndFeesAsync(requestNumber, delivery.Method, addressStr);
        }

        public async Task<DrivingLicenseResponseDto> CompleteRenewalAsync(string requestNumber)
        {
            var request = await _unitOfWork.Repository<ServiceRequest>().GetAsync(r => r.RequestNumber == requestNumber);
            if (request == null) throw new ValidationException("الطلب غير موجود.");

            var application = await _unitOfWork.Repository<RenewalApplication>().GetAsync(a => a.Id == request.ReferenceId);
            if (application == null) throw new ValidationException("طلب الرخصة غير موجود.");

            var license = await _unitOfWork.Repository<DrivingLicense>().GetAsync(l => l.Id == application.DrivingLicenseId);
            if (license == null) throw new ValidationException("الرخصة الأصلية غير موجودة.");

            // Update license details
            license.Category = application.RequestedCategory;
            license.IssueDate = DateOnly.FromDateTime(DateTime.UtcNow);
            license.ExpiryDate = license.IssueDate.AddYears(10); // Default 10 years for renewal
            license.IsPendingRenewal = false;

            _unitOfWork.Repository<DrivingLicense>().Update(license);
            application.Status = LicenseStatus.Completed;
            
            await _unitOfWork.CommitAsync();

            return _mapper.Map<DrivingLicenseResponseDto>(license);
        }

        private async Task<CitizenRegistry> GetCitizenAsync(string nationalId)
        {
            var citizen = await _unitOfWork.Repository<CitizenRegistry>().GetAsync(c => c.NationalId == nationalId);
            if (citizen == null) throw new ValidationException("المواطن غير موجود.", "CITIZEN_NOT_FOUND");
            return citizen;
        }
    }
}
