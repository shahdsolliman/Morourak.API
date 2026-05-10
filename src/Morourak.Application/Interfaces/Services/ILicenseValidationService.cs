using Morourak.Application.DTOs.Delivery;
using Morourak.Application.DTOs.DrivingLicenses;
using Morourak.Domain.Entities;
using Morourak.Domain.Enums.Driving;
using Morourak.Domain.Enums;
using Morourak.Domain.Enums.Common;
using Morourak.Application.Interfaces.Repositories;
using Morourak.Application.Exceptions;
using Microsoft.Extensions.Localization;

namespace Morourak.Application.Interfaces.Services
{
    public interface ILicenseValidationService
    {
        Task ValidateIssuanceEligibilityAsync(int citizenId, DrivingLicenseCategory requestedCategory);
        Task ValidateRenewalEligibilityAsync(DrivingLicense license, DrivingLicenseCategory requestedCategory);
        void ValidateDocuments(UploadDrivingLicenseDocumentsDto dto);
        void ValidateDelivery(DeliveryInfoDto delivery);
        Task ValidateReplacementEligibility(DrivingLicense oldLicense);
    }
}
