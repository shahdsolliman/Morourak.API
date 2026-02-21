using Morourak.Application.Interfaces;
using Morourak.Application.Interfaces.Services;
using Morourak.Domain.Entities;
using Morourak.Domain.Enums.Request;

namespace Morourak.Application.Services
{
    public class RequestNumberGenerator : IRequestNumberGenerator
    {
        private readonly IUnitOfWork _unitOfWork;

        public RequestNumberGenerator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<string> GenerateAsync(ServiceType serviceType)
        {
            var prefix = serviceType switch
            {
                ServiceType.VehicleLicenseIssue => "VL",
                ServiceType.VehicleLicenseRenewal => "VR",
                ServiceType.VehicleLicenseReplacementLost => "RPL",
                ServiceType.VehicleLicenseReplacementDamaged => "RPD",
                ServiceType.DrivingLicenseIssue => "DL",
                ServiceType.DrivingLicenseReplacementLost => "EL",
                ServiceType.DrivingLicenseReplacementDamaged => "ED",
                ServiceType.DrivingLicenseRenewal => "DR",
                ServiceType.ExaminationTechnical => "ET",
                ServiceType.ExaminationDriving => "EDR",
                _ => "SR"
            };

            // Query existing requests with the same prefix
            var requests = await _unitOfWork.Repository<ServiceRequest>().FindAsync(r => r.RequestNumber.StartsWith(prefix + "-"));
            
            var lastRequest = requests
                .OrderByDescending(r => r.SubmittedAt)
                .ThenByDescending(r => r.RequestNumber)
                .FirstOrDefault();

            int nextNumber;
            if (lastRequest != null)
            {
                var parts = lastRequest.RequestNumber.Split('-');
                if (parts.Length > 1 && int.TryParse(parts[1], out var lastNum))
                {
                    nextNumber = lastNum + 1;
                }
                else
                {
                    nextNumber = 1;
                }
            }
            else
            {
                // Default start numbers if none found
                nextNumber = serviceType switch
                {
                    ServiceType.VehicleLicenseIssue => 100,
                    ServiceType.DrivingLicenseIssue => 500,
                    ServiceType.DrivingLicenseRenewal => 800,
                    _ => 1
                };
            }

            return $"{prefix}-{nextNumber}";
        }
    }
}