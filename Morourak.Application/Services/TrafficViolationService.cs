using Morourak.Application.DTOs.Violations;
using Morourak.Application.Interfaces;
using Morourak.Application.Interfaces.Services;
using Morourak.Domain.Entities;
using Morourak.Domain.Enums.Violations;
using AppEx = Morourak.Application.Exceptions;

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
                .Select(MapToDto)
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

            return MapToDetailsDto(violation);
        }

        #endregion

        #region Payment Operations

        public async Task<PaymentResultDto> PaySingleViolationAsync(int violationId, decimal amount)
        {
            var repo = _unitOfWork.Repository<TrafficViolation>();
            var violation = await repo.GetByIdAsync(violationId);

            if (violation == null)
                throw new AppEx.ValidationException("المخالفة غير موجودة.", "VIOLATION_NOT_FOUND");
            if (!violation.IsPayable)
                throw new AppEx.ValidationException("هذه المخالفة لا يمكن سدادها عبر الإنترنت.", "VIOLATION_NOT_PAYABLE");
            if (violation.Status == ViolationStatus.Paid)
                throw new AppEx.ValidationException("تم سداد هذه المخالفة بالفعل.", "VIOLATION_ALREADY_PAID");

            var remaining = violation.FineAmount - violation.PaidAmount;
            if (amount > remaining)
                throw new AppEx.ValidationException($"المبلغ المدفوع ({amount:F2}) أكبر من المتبقي ({remaining:F2}).", "PAYMENT_EXCEEDS_BALANCE");

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
                if (violation == null) continue;
                if (!violation.IsPayable) continue;
                if (violation.Status == ViolationStatus.Paid) continue;

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
                MessageAr = $"تم سداد {paidCount} مخالفة. الإجمالي: {totalPaid:F2} جنيه مصري.",
                ViolationsPaid = paidCount,
                TotalAmountPaid = totalPaid,
                RemainingBalance = 0
            };
        }

        public async Task<PaymentResultDto> PayAllViolationsAsync(string licenseNumber, LicenseType licenseType)
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
            else
            {
                var license = await _unitOfWork.Repository<DrivingLicense>()
                    .FindAsync(l => l.LicenseNumber == licenseNumber);
                var first = license.FirstOrDefault();
                if (first == null)
                    throw new AppEx.ValidationException("رخصة القيادة غير موجودة.", "DRIVING_LICENSE_NOT_FOUND");
                licenseId = first.Id;
            }

            var repo = _unitOfWork.Repository<TrafficViolation>();
            var violations = await repo.FindAsync(v => v.RelatedLicenseId == licenseId && v.LicenseType == licenseType && v.Status != ViolationStatus.Paid && v.IsPayable);

            if (!violations.Any())
                return new PaymentResultDto
                {
                    Success = true,
                    MessageAr = "لا توجد مخالفات غير مدفوعة.",
                    ViolationsPaid = 0,
                    TotalAmountPaid = 0,
                    RemainingBalance = 0
                };

            decimal totalPaid = 0;
            foreach (var v in violations)
            {
                var remaining = v.FineAmount - v.PaidAmount;
                v.PaidAmount = v.FineAmount;
                v.Status = ViolationStatus.Paid;
                v.UpdatedAt = DateTime.UtcNow;

                repo.Update(v);
                totalPaid += remaining;
            }

            await _unitOfWork.CommitAsync();

            return new PaymentResultDto
            {
                Success = true,
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
                CitizenName = v.Citizen?.FirstName ?? "غير معروف",
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

        private static string GetStatusArabic(ViolationStatus status) => status switch
        {
            ViolationStatus.Unpaid => "غير مدفوعة",
            ViolationStatus.PartiallyPaid => "مدفوعة جزئياً",
            ViolationStatus.Paid => "مدفوعة",
            _ => "غير معروف"
        };

        private static string GetViolationTypeArabic(ViolationType type) => type switch
        {
            ViolationType.SpeedLimitExceeded => "تجاوز السرعة القصوى",
            ViolationType.RedLightViolation => "تجاوز الإشارة الحمراء",
            ViolationType.SeatBeltViolation => "عدم ربط حزام الأمان",
            ViolationType.IllegalParking => "وقوف غير قانوني",
            ViolationType.MobilePhoneUsage => "استخدام الهاتف أثناء القيادة",
            ViolationType.DrivingWithoutLicense => "القيادة بدون رخصة",
            ViolationType.ExpiredLicense => "القيادة برخصة منتهية",
            ViolationType.UnauthorizedModification => "تعديلات غير مصرح بها على المركبة",
            _ => "مخالفة مرورية"
        };

        #endregion
    }
}