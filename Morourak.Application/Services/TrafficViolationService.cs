using Morourak.Application.DTOs.Violations;
using Morourak.Application.Interfaces;
using Morourak.Application.Interfaces.Services;
using Morourak.Domain.Entities;
using Morourak.Domain.Enums.Violations;

namespace Morourak.Application.Services
{
    public class TrafficViolationService : ITrafficViolationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TrafficViolationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region Query Operations

        public async Task<ViolationListResponseDto> GetViolationsByLicenseAsync(int licenseId, LicenseType licenseType)
        {
            var repo = _unitOfWork.Repository<TrafficViolation>();

            var violations = await repo.FindAsync(
                v => v.RelatedLicenseId == licenseId && v.LicenseType == licenseType,
                v => v.Citizen);

            var violationDtos = violations
                .OrderByDescending(v => v.ViolationDateTime)
                .Select(MapToDto)
                .ToList();

            var unpaidViolations = violations
                .Where(v => v.Status != ViolationStatus.Paid && v.IsPayable)
                .ToList();

            var totalPayable = unpaidViolations.Sum(v => v.FineAmount - v.PaidAmount);
            var unpaidCount = unpaidViolations.Count;

            string message, messageAr;
            if (!violations.Any())
            {
                message = "No violations found for this license.";
                messageAr = "لا توجد مخالفات مرورية لهذه الرخصة.";
            }
            else if (unpaidCount == 0)
            {
                message = "All violations have been paid.";
                messageAr = "تم سداد جميع المخالفات المرورية.";
            }
            else
            {
                message = $"{unpaidCount} unpaid violation(s) found. Total payable: {totalPayable:F2} EGP.";
                messageAr = $"يوجد {unpaidCount} مخالفة غير مدفوعة. إجمالي المبلغ المستحق: {totalPayable:F2} جنيه مصري.";
            }

            return new ViolationListResponseDto
            {
                Violations = violationDtos,
                TotalCount = violations.Count,
                UnpaidCount = unpaidCount,
                TotalPayableAmount = totalPayable,
                Message = message,
                MessageAr = messageAr
            };
        }

        public async Task<ViolationDetailsDto> GetViolationDetailsAsync(int violationId)
        {
            var repo = _unitOfWork.Repository<TrafficViolation>();

            var violation = await repo.GetByIdAsync(violationId, v => v.Citizen);

            if (violation == null)
                throw new KeyNotFoundException($"Violation with ID {violationId} not found.");

            return MapToDetailsDto(violation);
        }

        #endregion

        #region Payment Operations

        public async Task<PaymentResultDto> PaySingleViolationAsync(int violationId, decimal amount)
        {
            var repo = _unitOfWork.Repository<TrafficViolation>();
            var violation = await repo.GetByIdAsync(violationId);

            if (violation == null)
                throw new KeyNotFoundException($"Violation with ID {violationId} not found.");

            if (!violation.IsPayable)
                throw new InvalidOperationException("This violation is not payable online.");

            if (violation.Status == ViolationStatus.Paid)
                throw new InvalidOperationException("This violation has already been paid.");

            var remaining = violation.FineAmount - violation.PaidAmount;

            if (amount > remaining)
                throw new InvalidOperationException(
                    $"Payment amount ({amount:F2}) exceeds remaining balance ({remaining:F2}).");

            violation.PaidAmount += amount;
            violation.Status = violation.PaidAmount >= violation.FineAmount
                ? ViolationStatus.Paid
                : ViolationStatus.PartiallyPaid;
            violation.UpdatedAt = DateTime.UtcNow;

            repo.Update(violation);
            await _unitOfWork.CommitAsync();

            var newRemaining = violation.FineAmount - violation.PaidAmount;

            return new PaymentResultDto
            {
                Success = true,
                Message = violation.Status == ViolationStatus.Paid
                    ? "Violation paid in full."
                    : $"Partial payment recorded. Remaining: {newRemaining:F2} EGP.",
                MessageAr = violation.Status == ViolationStatus.Paid
                    ? "تم سداد المخالفة بالكامل."
                    : $"تم تسجيل دفعة جزئية. المتبقي: {newRemaining:F2} جنيه مصري.",
                ViolationsPaid = 1,
                TotalAmountPaid = amount,
                RemainingBalance = newRemaining
            };
        }

        public async Task<PaymentResultDto> PaySelectedViolationsAsync(List<int> violationIds)
        {
            var repo = _unitOfWork.Repository<TrafficViolation>();
            decimal totalPaid = 0;
            int paidCount = 0;

            foreach (var id in violationIds)
            {
                var violation = await repo.GetByIdAsync(id);

                if (violation == null)
                    throw new KeyNotFoundException($"Violation with ID {id} not found.");

                if (!violation.IsPayable)
                    continue;

                if (violation.Status == ViolationStatus.Paid)
                    continue;

                var remaining = violation.FineAmount - violation.PaidAmount;
                violation.PaidAmount = violation.FineAmount;
                violation.Status = ViolationStatus.Paid;
                violation.UpdatedAt = DateTime.UtcNow;

                repo.Update(violation);
                totalPaid += remaining;
                paidCount++;
            }

            await _unitOfWork.CommitAsync();

            return new PaymentResultDto
            {
                Success = true,
                Message = $"{paidCount} violation(s) paid. Total: {totalPaid:F2} EGP.",
                MessageAr = $"تم سداد {paidCount} مخالفة. الإجمالي: {totalPaid:F2} جنيه مصري.",
                ViolationsPaid = paidCount,
                TotalAmountPaid = totalPaid,
                RemainingBalance = 0
            };
        }

        public async Task<PaymentResultDto> PayAllViolationsAsync(int licenseId, LicenseType licenseType)
        {
            var repo = _unitOfWork.Repository<TrafficViolation>();

            var violations = await repo.FindAsync(
                v => v.RelatedLicenseId == licenseId
                     && v.LicenseType == licenseType
                     && v.Status != ViolationStatus.Paid
                     && v.IsPayable);

            if (!violations.Any())
            {
                return new PaymentResultDto
                {
                    Success = true,
                    Message = "No unpaid violations to pay.",
                    MessageAr = "لا توجد مخالفات غير مدفوعة.",
                    ViolationsPaid = 0,
                    TotalAmountPaid = 0,
                    RemainingBalance = 0
                };
            }

            decimal totalPaid = 0;

            foreach (var violation in violations)
            {
                var remaining = violation.FineAmount - violation.PaidAmount;
                violation.PaidAmount = violation.FineAmount;
                violation.Status = ViolationStatus.Paid;
                violation.UpdatedAt = DateTime.UtcNow;

                repo.Update(violation);
                totalPaid += remaining;
            }

            await _unitOfWork.CommitAsync();

            return new PaymentResultDto
            {
                Success = true,
                Message = $"All {violations.Count} violation(s) paid. Total: {totalPaid:F2} EGP.",
                MessageAr = $"تم سداد جميع المخالفات ({violations.Count} مخالفة). الإجمالي: {totalPaid:F2} جنيه مصري.",
                ViolationsPaid = violations.Count,
                TotalAmountPaid = totalPaid,
                RemainingBalance = 0
            };
        }

        #endregion

        #region Mapping Helpers

        private static ViolationDto MapToDto(TrafficViolation v)
        {
            return new ViolationDto
            {
                ViolationId = v.Id,
                ViolationNumber = v.ViolationNumber,
                ViolationType = v.ViolationType.ToString(),
                LegalReference = v.LegalReference,
                Description = v.Description,
                Location = v.Location,
                ViolationDateTime = v.ViolationDateTime,
                FineAmount = v.FineAmount,
                PaidAmount = v.PaidAmount,
                Status = v.Status.ToString(),
                StatusAr = GetStatusArabic(v.Status),
                IsPayable = v.IsPayable
            };
        }

        private static ViolationDetailsDto MapToDetailsDto(TrafficViolation v)
        {
            return new ViolationDetailsDto
            {
                ViolationId = v.Id,
                ViolationNumber = v.ViolationNumber,
                CitizenName = v.Citizen?.NameAr ?? "غير معروف",
                NationalId = v.Citizen?.NationalId ?? "غير معروف",
                LicenseType = v.LicenseType.ToString(),
                LicenseTypeAr = v.LicenseType == LicenseType.Driving ? "رخصة قيادة" : "رخصة مركبة",
                RelatedLicenseId = v.RelatedLicenseId,
                ViolationType = v.ViolationType.ToString(),
                ViolationTypeAr = GetViolationTypeArabic(v.ViolationType),
                LegalReference = v.LegalReference,
                Description = v.Description,
                Location = v.Location,
                ViolationDateTime = v.ViolationDateTime,
                FineAmount = v.FineAmount,
                PaidAmount = v.PaidAmount,
                Status = v.Status.ToString(),
                StatusAr = GetStatusArabic(v.Status),
                IsPayable = v.IsPayable
            };
        }

        private static string GetStatusArabic(ViolationStatus status)
        {
            return status switch
            {
                ViolationStatus.Unpaid => "غير مدفوعة",
                ViolationStatus.PartiallyPaid => "مدفوعة جزئياً",
                ViolationStatus.Paid => "مدفوعة",
                _ => "غير معروف"
            };
        }

        private static string GetViolationTypeArabic(Domain.Enums.Violations.ViolationType type)
        {
            return type switch
            {
                Domain.Enums.Violations.ViolationType.SpeedLimitExceeded => "تجاوز السرعة القصوى",
                Domain.Enums.Violations.ViolationType.RedLightViolation => "تجاوز الإشارة الحمراء",
                Domain.Enums.Violations.ViolationType.SeatBeltViolation => "عدم ربط حزام الأمان",
                Domain.Enums.Violations.ViolationType.IllegalParking => "وقوف غير قانوني",
                Domain.Enums.Violations.ViolationType.MobilePhoneUsage => "استخدام الهاتف أثناء القيادة",
                Domain.Enums.Violations.ViolationType.DrivingWithoutLicense => "القيادة بدون رخصة",
                Domain.Enums.Violations.ViolationType.ExpiredLicense => "القيادة برخصة منتهية",
                Domain.Enums.Violations.ViolationType.UnauthorizedModification => "تعديلات غير مصرح بها على المركبة",
                _ => "مخالفة مرورية"
            };
        }

        #endregion
    }
}
