using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Morourak.Domain.Entities;
using Morourak.Domain.Enums.Violations;

namespace Morourak.Infrastructure.Persistence.SeedData
{
    public static class TrafficViolationSeeder
    {
        public static async Task SeedAsync(PersistenceDbContext context, ILogger logger)
        {
            var filePath = Path.Combine(
                AppContext.BaseDirectory,
                "Persistence",
                "SeedData",
                "traffic-violations.json");

            if (!File.Exists(filePath))
            {
                logger.LogWarning("Traffic violations seed file not found at {Path}", filePath);
                return;
            }

            var jsonData = await File.ReadAllTextAsync(filePath);
            var seedItems = JsonSerializer.Deserialize<List<TrafficViolationSeedModel>>(jsonData);

            if (seedItems == null || !seedItems.Any())
            {
                logger.LogInformation("No traffic violations in seed file.");
                return;
            }

            var existingNumbers = await context.TrafficViolations
                .Select(v => v.ViolationNumber)
                .ToListAsync();

            // Build lookup dictionaries
            var citizenByNationalId = await context.CitizenRegistries
                .ToDictionaryAsync(c => c.NationalId, c => c.Id);

            var drivingLicenseByNumber = await context.DrivingLicenses
                .ToDictionaryAsync(d => d.LicenseNumber, d => d.Id);

            var vehicleLicenseByNumber = await context.VehicleLicenses
                .ToDictionaryAsync(v => v.VehicleLicenseNumber, v => v.Id);

            var toAdd = new List<TrafficViolation>();

            foreach (var item in seedItems)
            {
                if (existingNumbers.Contains(item.ViolationNumber))
                    continue;

                if (!citizenByNationalId.TryGetValue(item.CitizenNationalId, out var citizenId))
                {
                    logger.LogWarning(
                        "Citizen with NationalId {NationalId} not found; skipping violation {ViolationNumber}.",
                        item.CitizenNationalId, item.ViolationNumber);
                    continue;
                }

                var licenseType = ParseEnumOrDefault(item.LicenseType, LicenseType.Driving);

                int relatedLicenseId;
                if (licenseType == LicenseType.Driving)
                {
                    if (!drivingLicenseByNumber.TryGetValue(item.RelatedLicenseNumber, out relatedLicenseId))
                    {
                        logger.LogWarning(
                            "Driving license {License} not found; skipping violation {ViolationNumber}.",
                            item.RelatedLicenseNumber, item.ViolationNumber);
                        continue;
                    }
                }
                else
                {
                    if (!vehicleLicenseByNumber.TryGetValue(item.RelatedLicenseNumber, out relatedLicenseId))
                    {
                        logger.LogWarning(
                            "Vehicle license {License} not found; skipping violation {ViolationNumber}.",
                            item.RelatedLicenseNumber, item.ViolationNumber);
                        continue;
                    }
                }

                toAdd.Add(new TrafficViolation
                {
                    ViolationNumber = item.ViolationNumber,
                    CitizenRegistryId = citizenId,
                    RelatedLicenseId = relatedLicenseId,
                    LicenseType = licenseType,
                    ViolationType = ParseEnumOrDefault(item.ViolationType, Domain.Enums.Violations.ViolationType.SpeedLimitExceeded),
                    LegalReference = item.LegalReference,
                    Description = item.Description,
                    Location = item.Location,
                    ViolationDateTime = item.ViolationDateTime,
                    FineAmount = item.FineAmount,
                    PaidAmount = item.PaidAmount,
                    Status = ParseEnumOrDefault(item.Status, ViolationStatus.Unpaid),
                    IsPayable = item.IsPayable
                });
            }

            if (!toAdd.Any())
            {
                logger.LogInformation("Traffic violations already seeded; no new records.");
                return;
            }

            await context.TrafficViolations.AddRangeAsync(toAdd);
            await context.SaveChangesAsync();

            logger.LogInformation("Seeded {Count} traffic violations.", toAdd.Count);
        }

        private static TEnum ParseEnumOrDefault<TEnum>(string? value, TEnum defaultValue)
            where TEnum : struct, Enum
        {
            return Enum.TryParse<TEnum>(value, true, out var result)
                ? result
                : defaultValue;
        }
    }
}
