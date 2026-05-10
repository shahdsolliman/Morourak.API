using Morourak.Application.DTOs.Delivery;
using Morourak.Application.DTOs;
using Morourak.Application.Interfaces.Services;
using Morourak.Application.Interfaces;
using Morourak.Domain.Entities;
using Morourak.Domain.Enums;
using Morourak.Domain.Enums.Request;
using Morourak.Domain.Enums.Common;
using Morourak.Application.Exceptions;
using AutoMapper;

namespace Morourak.Application.Services.Licenses
{
    public class DrivingLicenseReplacementService : IDrivingLicenseReplacementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILicenseValidationService _validationService;
        private readonly IRequestNumberGenerator _generator;
        private readonly IMapper _mapper;
        private readonly IServiceRequestService _serviceRequestService;

        public DrivingLicenseReplacementService(
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

        public async Task<ServiceRequestDto> IssueReplacementAsync(
            string nationalId,
            string drivingLicenseNumber,
            ReplacementType replacementType,
            DeliveryInfoDto delivery)
        {
            var citizen = await GetCitizenAsync(nationalId);

            var oldLicense = await _unitOfWork.Repository<DrivingLicense>().GetAsync(
                l => l.CitizenRegistryId == citizen.Id &&
                     l.LicenseNumber == drivingLicenseNumber);

            if (oldLicense == null)
                throw new ValidationException("الرخصة غير موجودة.", "LICENSE_NOT_FOUND");

            await _validationService.ValidateReplacementEligibility(oldLicense);
            _validationService.ValidateDelivery(delivery);

            var serviceType = replacementType switch
            {
                ReplacementType.Lost => ServiceType.DrivingLicenseReplacementLost,
                ReplacementType.Damaged => ServiceType.DrivingLicenseReplacementDamaged,
                _ => throw new ValidationException("نوع البدل غير مدعوم.", "INVALID_REPLACEMENT_TYPE")
            };

            var serviceRequest = new ServiceRequest
            {
                ReferenceId = oldLicense.Id,
                ServiceType = serviceType,
                Status = RequestStatus.Pending,
                CitizenNationalId = nationalId,
                RequestNumber = await _generator.GenerateAsync(serviceType),
                SubmittedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            // Set delivery/fees and commit the full transaction in one go
            return await _serviceRequestService.SetDeliveryAndFeesAsync(
                serviceRequest, 
                delivery.Method, 
                FormatAddress(delivery.Address));
        }

        public async Task<DrivingLicenseResponseDto> CompleteReplacementAsync(string requestNumber)
        {
            var request = await _unitOfWork.Repository<ServiceRequest>().GetAsync(r => r.RequestNumber == requestNumber);
            if (request == null) throw new ValidationException("الطلب غير موجود.");

            var oldLicense = await _unitOfWork.Repository<DrivingLicense>().GetAsync(l => l.Id == request.ReferenceId);
            if (oldLicense == null) throw new ValidationException("الرخصة الأصلية غير موجودة.");

            // Create new replacement license
            var newLicense = new DrivingLicense
            {
                CitizenRegistryId = oldLicense.CitizenRegistryId,
                LicenseNumber = $"DL-R-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                Category = oldLicense.Category,
                IssueDate = DateOnly.FromDateTime(DateTime.UtcNow),
                ExpiryDate = oldLicense.ExpiryDate,
                IsReplaced = false
            };

            await _unitOfWork.Repository<DrivingLicense>().AddAsync(newLicense);

            // Invalidate old license
            oldLicense.IsReplaced = true;
            _unitOfWork.Repository<DrivingLicense>().Update(oldLicense);

            await _unitOfWork.CommitAsync();

            return _mapper.Map<DrivingLicenseResponseDto>(newLicense);
        }

        private async Task<CitizenRegistry> GetCitizenAsync(string nationalId)
        {
            var citizen = await _unitOfWork.Repository<CitizenRegistry>().GetAsync(c => c.NationalId == nationalId);
            if (citizen == null) throw new ValidationException("المواطن غير موجود.", "CITIZEN_NOT_FOUND");
            return citizen;
        }

        private string? FormatAddress(AddressDto? address)
        {
            if (address == null) return null;
            return $"{address.Governorate}, {address.City}, {address.Details}";
        }
    }
}
