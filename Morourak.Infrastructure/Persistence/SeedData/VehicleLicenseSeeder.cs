using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Morourak.Domain.Entities;
using Morourak.Domain.Enums;
using Morourak.Domain.Enums.Vehicles;
using Morourak.Domain.Enums.Common;

namespace Morourak.Infrastructure.Persistence.SeedData
{
    public static class VehicleLicenseSeeder
    {
        public static async Task SeedAsync(PersistenceDbContext context, ILogger logger)
        {
            var filePath = Path.Combine(
                AppContext.BaseDirectory,
                "Persistence",
                "SeedData",
                "vehicle-licenses.json");

            if (!File.Exists(filePath))
            {
                logger.LogWarning("Vehicle licenses seed file not found at {Path}", filePath);
                return;
            }

            var jsonData = await File.ReadAllTextAsync(filePath);
            var licenses = JsonSerializer.Deserialize<List<VehicleLicenseSeedModel>>(jsonData);

            if (licenses == null || !licenses.Any())
            {
                logger.LogInformation("No vehicle licenses in seed file.");
                return;
            }

            var existingNumbers = await context.VehicleLicenses
                .Select(v => v.VehicleLicenseNumber)
                .ToListAsync();

            var citizenByNationalId = await context.CitizenRegistries
                .ToDictionaryAsync(c => c.NationalId, c => c.Id);

            var toAdd = new List<VehicleLicense>();

            var lastLicense = await context.VehicleLicenses
                .OrderByDescending(l => l.Id)
                .FirstOrDefaultAsync();

            int lastSeq = 200000;

            if (lastLicense != null && !string.IsNullOrEmpty(lastLicense.ChassisNumber))
            {
                var chassisStr = lastLicense.ChassisNumber;
                if (chassisStr.StartsWith("CHS") && int.TryParse(chassisStr.Substring(3), out var parsedSeq))
                {
                    lastSeq = parsedSeq;
                }
            }

            foreach (var l in licenses)
            {
                if (existingNumbers.Contains(l.VehicleLicenseNumber))
                    continue;

                if (!citizenByNationalId.TryGetValue(l.CitizenNationalId, out var citizenId))
                {
                    logger.LogWarning(
                        "Citizen with NationalId {NationalId} not found; skipping vehicle license {VehicleLicenseNumber}.",
                        l.CitizenNationalId, l.VehicleLicenseNumber);
                    continue;
                }

                lastSeq++; 

                toAdd.Add(new VehicleLicense
                {
                    VehicleLicenseNumber = l.VehicleLicenseNumber,
                    CitizenRegistryId = citizenId,
                    PlateNumber = l.PlateNumber,
                    Brand = l.Brand,
                    Model = l.Model,
                    IssueDate = l.IssueDate,
                    ExpiryDate = l.ExpiryDate,

                    ChassisNumber = $"CHS{lastSeq:D6}",
                    EngineNumber = $"ENG{lastSeq:D6}",

                    ExaminationId = null,
                    VehicleType = ParseEnumOrDefault(l.VehicleType, VehicleType.PrivateCar),
                    DeliveryMethod = ParseEnumOrDefault(l.DeliveryMethod, DeliveryMethod.HomeDelivery)
                });
            }

            if (!toAdd.Any())
            {
                logger.LogInformation("Vehicle licenses already seeded; no new records.");
                return;
            }

            await context.VehicleLicenses.AddRangeAsync(toAdd);
            await context.SaveChangesAsync();

            logger.LogInformation("Seeded {Count} vehicle licenses.", toAdd.Count);
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