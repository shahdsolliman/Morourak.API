using Morourak.Application.DTOs.Violations;
using Morourak.Application.Interfaces;
using Morourak.Application.Interfaces.Services;
using Morourak.Domain.Entities;
using Morourak.Domain.Enums.Violations;
using System.Globalization;
using Microsoft.Extensions.Logging;
using AutoMapper;
using AppEx = Morourak.Application.Exceptions;

namespace Morourak.Application.Services
{
    public class TrafficViolationService : ITrafficViolationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TrafficViolationService> _logger;
        private readonly IMapper _mapper;

        public TrafficViolationService(IUnitOfWork unitOfWork, ILogger<TrafficViolationService> logger, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<ViolationListResponseDto> GetViolationsByLicenseNumberAsync(string licenseNumber, LicenseType licenseType)
        {
            if (string.IsNullOrWhiteSpace(licenseNumber))
                throw new AppEx.ValidationException("رقم الرخصة مطلوب.", "LICENSE_NUMBER_REQUIRED");

            int licenseId;
            if (licenseType == LicenseType.Vehicle)
            {
                var license = await _unitOfWork.Repository<VehicleLicense>()
                    .FindAsync(l => l.VehicleLicenseNumber == licenseNumber);
                var first = license.FirstOrDefault();
                if (first == null)
                    throw new AppEx.ValidationException("رخصة المركبة غير موجودة.", "VEHICLE_LICENSE_NOT_FOUND");
                licenseId = first.Id;
            }
            else // Driving
            {
                var license = await _unitOfWork.Repository<DrivingLicense>()
                    .FindAsync(l => l.LicenseNumber == licenseNumber);
                var first = license.FirstOrDefault();
                if (first == null)
                    throw new AppEx.ValidationException("رخصة القيادة غير موجودة.", "DRIVING_LICENSE_NOT_FOUND");
                licenseId = first.Id;
            }

            var repo = _unitOfWork.Repository<TrafficViolation>();
            var violations = await repo.FindAsync(v => v.RelatedLicenseId == licenseId && v.LicenseType == licenseType, v => v.Citizen);

            var violationDtos = violations
                .OrderByDescending(v => v.ViolationDateTime)
                .Select(v => _mapper.Map<ViolationDto>(v))
                .ToList();

            var unpaid = violations.Where(v => v.Status != ViolationStatus.Paid && v.IsPayable).ToList();
            var totalPayable = unpaid.Sum(v => v.FineAmount - v.PaidAmount);

            string messageAr = !violations.Any()
                ? "لا توجد مخالفات لهذه الرخصة."
                : unpaid.Count == 0
                    ? "تم سداد جميع المخالفات."
                    : $"يوجد {unpaid.Count} مخالفة غير مدفوعة. إجمالي المبلغ المستحق: {totalPayable:F2} جنيه مصري.";

            return new ViolationListResponseDto
            {
                Violations = violationDtos,
                TotalCount = violations.Count,
                UnpaidCount = unpaid.Count,
                TotalPayableAmount = totalPayable,
                MessageAr = messageAr
            };
        }

        public async Task<ViolationDetailsDto> GetViolationDetailsAsync(string licenseNumber, LicenseType licenseType)
        {
            if (string.IsNullOrWhiteSpace(licenseNumber))
                throw new AppEx.ValidationException("رقم الرخصة مطلوب.", "LICENSE_NUMBER_REQUIRED");

            int licenseId;
            if (licenseType == LicenseType.Vehicle)
            {
                var license = await _unitOfWork.Repository<VehicleLicense>()
                    .FindAsync(l => l.VehicleLicenseNumber == licenseNumber);
                var first = license.FirstOrDefault();
                if (first == null)
                    throw new AppEx.ValidationException("رخصة المركبة غير موجودة.", "VEHICLE_LICENSE_NOT_FOUND");
                licenseId = first.Id;
            }
            else // Driving
            {
                var license = await _unitOfWork.Repository<DrivingLicense>()
                    .FindAsync(l => l.LicenseNumber == licenseNumber);
                var first = license.FirstOrDefault();
                if (first == null)
                    throw new AppEx.ValidationException("رخصة القيادة غير موجودة.", "DRIVING_LICENSE_NOT_FOUND");
                licenseId = first.Id;
            }

            var repo = _unitOfWork.Repository<TrafficViolation>();
            var violation = (await repo.FindAsync(v => v.RelatedLicenseId == licenseId && v.LicenseType == licenseType, v => v.Citizen))
                            .OrderByDescending(v => v.ViolationDateTime)
                            .FirstOrDefault();

            if (violation == null)
                throw new AppEx.ValidationException("لا توجد مخالفات لهذه الرخصة.", "VIOLATION_NOT_FOUND");

            return _mapper.Map<ViolationDetailsDto>(violation);
        }

        public Task<PaymentResultDto> PaySingleViolationAsync(int violationId, decimal amount)
        {
            _logger.LogWarning("Direct payment attempted for violation {ViolationId}. Redirecting to PaymentService.", violationId);
            return Task.FromResult(new PaymentResultDto
            {
                Success = false,
                MessageAr = "يرجى استخدام خدمة الدفع الموحدة لإتمام العملية.",
                Message = "Please use the unified payment service to complete this operation."
            });
        }

        public Task<PaymentResultDto> PaySelectedViolationsAsync(List<int> violationIds)
        {
            _logger.LogWarning("Direct payment attempted for selected violations. Redirecting to PaymentService.");
            return Task.FromResult(new PaymentResultDto
            {
                Success = false,
                MessageAr = "يرجى استخدام خدمة الدفع الموحدة لإتمام العملية.",
                Message = "Please use the unified payment service to complete this operation."
            });
        }

        public Task<PaymentResultDto> PayAllViolationsAsync(string licenseNumber, LicenseType licenseType)
        {
            _logger.LogWarning("Direct payment attempted for all violations on license {LicenseNumber}. Redirecting to PaymentService.", licenseNumber);
            return Task.FromResult(new PaymentResultDto
            {
                Success = false,
                MessageAr = "يرجى استخدام خدمة الدفع الموحدة لإتمام العملية.",
                Message = "Please use the unified payment service to complete this operation."
            });
        }

    }
}