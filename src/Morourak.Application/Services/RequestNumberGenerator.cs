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

            var year = DateTime.UtcNow.Year;
            var fullPrefix = $"{prefix}-{year}";

            var requestsWithPrefix = await repository
                .FindAsync(r => r.RequestNumber.StartsWith(fullPrefix + "-"));

            var nextNumber = requestsWithPrefix
                .Select(r => TryParseSuffix(r.RequestNumber, fullPrefix))
                .Where(n => n.HasValue)
                .Select(n => n!.Value)
                .DefaultIfEmpty(startNumber - 1)
                .Max() + 1;

            var generated = $"{fullPrefix}-{nextNumber}";

            // Defensive collision check (handles concurrency edge cases)
            while ((await repository.FindAsync(r => r.RequestNumber == generated)).Any())
            {
                nextNumber++;
                generated = $"{fullPrefix}-{nextNumber}";
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

        private static int? TryParseSuffix(string requestNumber, string fullPrefix)
        {
            if (!requestNumber.StartsWith(fullPrefix + "-"))
                return null;

            var suffix = requestNumber[(fullPrefix.Length + 1)..];
            return int.TryParse(suffix, out var parsed) ? parsed : null;
        }
    }
}