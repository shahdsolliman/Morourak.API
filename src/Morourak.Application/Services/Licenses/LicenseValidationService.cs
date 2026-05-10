using Morourak.Application.Interfaces.Services;
using Morourak.Application.Interfaces.Repositories;
using Morourak.Domain.Entities;
using Morourak.Domain.Enums.Driving;
using Morourak.Domain.Enums;
using Morourak.Domain.Enums.Common;
using Morourak.Application.Exceptions;
using Morourak.Application.DTOs.DrivingLicenses;
using Morourak.Application.DTOs.Delivery;
using Microsoft.Extensions.Localization;
using Morourak.Application.Interfaces;
using Morourak.Domain.Extensions;
using Morourak.Domain.Enums.Violations;

namespace Morourak.Application.Services.Licenses
{
    public class LicenseValidationService : ILicenseValidationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStringLocalizer<LicenseValidationService> _localizer;

        public LicenseValidationService(IUnitOfWork unitOfWork, IStringLocalizer<LicenseValidationService> localizer)
        {
            _unitOfWork = unitOfWork;
            _localizer = localizer;
        }

        public async Task ValidateIssuanceEligibilityAsync(int citizenId, DrivingLicenseCategory requestedCategory)
        {
            var licenseRepo = _unitOfWork.Repository<DrivingLicense>();
            
            // 1. Check existing licenses for the same category
            var existingLicenses = await licenseRepo.FindAsync(l => l.CitizenRegistryId == citizenId);
            var sameCategoryLicense = existingLicenses.FirstOrDefault(l => l.Category == requestedCategory);

            if (sameCategoryLicense != null)
            {
                if (sameCategoryLicense.CurrentStatus == LicenseStatus.Active)
                    throw new ValidationException(_localizer["LicenseAlreadyExists", requestedCategory.GetDisplayName()], "LICENSE_ALREADY_EXISTS");
                
                if (sameCategoryLicense.CurrentStatus == LicenseStatus.Expired)
                    throw new ValidationException(_localizer["LicenseExpiredSpecific", requestedCategory.GetDisplayName()], "LICENSE_EXPIRED");

                if (sameCategoryLicense.CurrentStatus == LicenseStatus.Withdrawn)
                    throw new ValidationException(_localizer["LicenseWithdrawnSpecific", requestedCategory.GetDisplayName()], "LICENSE_WITHDRAWN");
            }

            // 2. Check pending/active applications for the same category
            var activeApplication = (await _unitOfWork.Repository<DrivingLicenseApplication>()
                .FindAsync(a => a.CitizenRegistryId == citizenId && 
                                a.Category == requestedCategory && 
                                a.Status != LicenseStatus.Completed))
                .FirstOrDefault();

            if (activeApplication != null)
                throw new ValidationException(_localizer["ApplicationAlreadyInProgress", requestedCategory.GetDisplayName()], "APPLICATION_IN_PROGRESS");
        }

        public async Task ValidateRenewalEligibilityAsync(DrivingLicense license, DrivingLicenseCategory requestedCategory)
        {
            if (license.CurrentStatus != LicenseStatus.Expired)
                throw new ValidationException(_localizer["LicenseStillValid"], "LICENSE_STILL_VALID");
            
            if (license.CurrentStatus == LicenseStatus.Withdrawn)
                throw new ValidationException(_localizer["LicenseWithdrawnCannotRenew"], "LICENSE_WITHDRAWN");

            // Check for unpaid violations
            var hasViolations = (await _unitOfWork.Repository<TrafficViolation>()
                .FindAsync(v => v.RelatedLicenseId == license.Id && 
                                v.LicenseType == LicenseType.Driving && 
                                v.Status != ViolationStatus.Paid && 
                                v.IsPayable))
                .Any();

            if (hasViolations)
                throw new ValidationException("لا يمكن التجديد لوجود مخالفات غير مدفوعة. يرجى السداد أولاً.", "UNPAID_VIOLATIONS");
            
            var pendingRenewal = (await _unitOfWork.Repository<RenewalApplication>()
                .FindAsync(r => r.DrivingLicenseId == license.Id && r.Status == LicenseStatus.Pending))
                .FirstOrDefault();

            if (pendingRenewal != null)
                throw new ValidationException(_localizer["RenewalPending"], "RENEWAL_PENDING");
        }

        public void ValidateDocuments(UploadDrivingLicenseDocumentsDto dto)
        {
            if (dto.PersonalPhoto == null) throw new ValidationException(_localizer["PhotoRequired"], "DOCUMENT_MISSING");
            if (dto.EducationalCertificate == null) throw new ValidationException(_localizer["EduRequired"], "DOCUMENT_MISSING");
            if (dto.IdCard == null) throw new ValidationException(_localizer["IdCardRequired"], "DOCUMENT_MISSING");
        }

        public void ValidateDelivery(DeliveryInfoDto delivery)
        {
            if (delivery == null) throw new ValidationException(_localizer["DeliveryMissing"], "DELIVERY_MISSING");
            if (delivery.Method == DeliveryMethod.HomeDelivery)
            {
                if (delivery.Address == null) throw new ValidationException(_localizer["AddressRequired"], "ADDRESS_MISSING");
            }
        }

        public async Task ValidateReplacementEligibility(DrivingLicense oldLicense)
        {
            if (oldLicense.CurrentStatus != LicenseStatus.Active)
                throw new ValidationException(_localizer["LicenseNotReplaceable"], "LICENSE_NOT_REPLACEABLE");

            // Check for unpaid violations
            var hasViolations = (await _unitOfWork.Repository<TrafficViolation>()
                .FindAsync(v => v.RelatedLicenseId == oldLicense.Id && 
                                v.LicenseType == LicenseType.Driving && 
                                v.Status != ViolationStatus.Paid && 
                                v.IsPayable))
                .Any();

            if (hasViolations)
                throw new ValidationException("لا يمكن استخراج بدل فاقد/تالف لوجود مخالفات غير مدفوعة. يرجى السداد أولاً.", "UNPAID_VIOLATIONS");
        }
    }
}
