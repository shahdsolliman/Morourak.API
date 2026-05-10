using Morourak.Application.DTOs.Delivery;
using Morourak.Application.DTOs.DrivingLicenses;
using Morourak.Application.DTOs.Licenses;
using Morourak.Application.Interfaces;
using Morourak.Application.Interfaces.Services;
using Morourak.Domain.Entities;
using Morourak.Domain.Enums.Appointments;
using Morourak.Domain.Enums.Common;
using Morourak.Application.Services.Licenses;
using AutoMapper;
using Morourak.Domain.Enums.Request;
using System.ComponentModel.DataAnnotations;

namespace Morourak.Application.Services.Licenses
{
    /// <summary>
    /// A Facade service that delegates specialized tasks to focused sub-services.
    /// This preserves existing API compatibility while satisfying SRP.
    /// </summary>
    public class DrivingLicenseService : IDrivingLicenseService
    {
        private readonly IDrivingLicenseIssuanceService _issuanceService;
        private readonly IDrivingLicenseRenewalService _renewalService;
        private readonly IDrivingLicenseReplacementService _replacementService;
        private readonly IDrivingLicenseQueryService _queryService;
        private readonly IDrivingLicenseResultService _resultService;

        public DrivingLicenseService(
            IDrivingLicenseIssuanceService issuanceService,
            IDrivingLicenseRenewalService renewalService,
            IDrivingLicenseReplacementService replacementService,
            IDrivingLicenseQueryService queryService,
            IDrivingLicenseResultService resultService)
        {
            _issuanceService = issuanceService;
            _renewalService = renewalService;
            _replacementService = replacementService;
            _queryService = queryService;
            _resultService = resultService;
        }

        public Task<DrivingLicenseApplicationDto> UploadInitialDocumentsAsync(string nationalId, UploadDrivingLicenseDocumentsDto dto)
            => _issuanceService.UploadInitialDocumentsAsync(nationalId, dto);

        public Task<RenewalApplicationDto> SubmitRenewalRequestAsync(string nationalId, SubmitRenewalRequestDto dto)
            => _renewalService.SubmitRenewalRequestAsync(nationalId, dto);

        public Task<Morourak.Application.DTOs.ServiceRequestDto> IssueReplacementAsync(string nationalId, string drivingLicenseNumber, ReplacementType replacementType, DeliveryInfoDto delivery)
            => _replacementService.IssueReplacementAsync(nationalId, drivingLicenseNumber, replacementType, delivery);

        public Task<IEnumerable<DrivingLicenseDto>> GetAllLicensesByCitizenAsync(string nationalId)
            => _queryService.GetAllLicensesByCitizenAsync(nationalId);

        public Task<DrivingLicenseApplicationDto> GetApplicationByIdAsync(int applicationId, string nationalId)
            => _queryService.GetApplicationByIdAsync(applicationId, nationalId);

        public Task SubmitAppointmentResultAsync(int applicationId, AppointmentType type, bool passed, string? notes)
            => _resultService.SubmitAppointmentResultAsync(applicationId, type, passed, notes);

        // Delegation for other complex flows...
        public Task<Morourak.Application.DTOs.ServiceRequestDto> FinalizeLicenseAsync(string requestNumber, string nationalId, DeliveryInfoDto delivery)
             => _issuanceService.FinalizeLicenseAsync(requestNumber, nationalId, delivery);

        public Task<Morourak.Application.DTOs.ServiceRequestDto> FinalizeRenewalAsync(string requestNumber, string nationalId, DeliveryInfoDto delivery)
            => _renewalService.FinalizeRenewalAsync(requestNumber, nationalId, delivery);

        public async Task<DrivingLicenseResponseDto> CompleteIssuanceAsync(string requestNumber)
        {
            var request = await _queryService.GetRequestByNumberAsync(requestNumber);
            if (request == null) throw new ValidationException("الطلب غير موجود.");

            return request.ServiceType switch
            {
                ServiceType.DrivingLicenseIssue => await _issuanceService.CompleteIssuanceAsync(requestNumber),
                ServiceType.DrivingLicenseRenewal => await _renewalService.CompleteRenewalAsync(requestNumber),
                ServiceType.DrivingLicenseReplacementLost or ServiceType.DrivingLicenseReplacementDamaged 
                    => await _replacementService.CompleteReplacementAsync(requestNumber),
                _ => throw new ValidationException("نوع الخدمة غير مدعوم للاستكمال التلقائي.")
            };
        }
    }
}
