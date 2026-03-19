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
            var (prefix, startNumber) = GetPrefixAndStart(serviceType);
            var repository = _unitOfWork.Repository<ServiceRequest>();

            var requestsWithPrefix = await repository.FindAsync(r => r.RequestNumber.StartsWith(prefix + "-"));

            var nextNumber = requestsWithPrefix
                .Select(r => TryParseSuffix(r.RequestNumber, prefix))
                .Where(n => n.HasValue)
                .Select(n => n!.Value)
                .DefaultIfEmpty(startNumber - 1)
                .Max() + 1;

            var generated = $"{prefix}-{nextNumber}";

            // Defensive collision check against malformed or concurrent inserts already in DB.
            while ((await repository.FindAsync(r => r.RequestNumber == generated)).Any())
            {
                nextNumber++;
                generated = $"{prefix}-{nextNumber}";
            }

            return generated;
        }

        private static (string Prefix, int StartNumber) GetPrefixAndStart(ServiceType serviceType)
        {
            return serviceType switch
            {
                ServiceType.VehicleLicenseIssue => ("VL", 100),
                ServiceType.VehicleLicenseRenewal => ("VR", 200),
                ServiceType.VehicleLicenseReplacementLost => ("RPL", 300),
                ServiceType.VehicleLicenseReplacementDamaged => ("RPD", 400),
                ServiceType.DrivingLicenseIssue => ("DL", 500),
                ServiceType.DrivingLicenseRenewal => ("DR", 800),
                ServiceType.DrivingLicenseReplacementLost => ("EL", 900),
                ServiceType.DrivingLicenseReplacementDamaged => ("ED", 1000),
                _ => ("SR", 1)
            };
        }

        private static int? TryParseSuffix(string requestNumber, string prefix)
        {
            if (!requestNumber.StartsWith(prefix + "-"))
                return null;

            var suffix = requestNumber[(prefix.Length + 1)..];
            return int.TryParse(suffix, out var parsed) ? parsed : null;
        }
    }
}
