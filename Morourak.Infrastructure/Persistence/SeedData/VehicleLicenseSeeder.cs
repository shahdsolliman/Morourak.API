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

            // ? ????? Id ??????? ???? ?????? int
            var citizenByNationalId = await context.CitizenRegistries
                .ToDictionaryAsync(c => c.NationalId, c => c.Id);

            var toAdd = new List<VehicleLicense>();

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

                toAdd.Add(new VehicleLicense
                {
                    VehicleLicenseNumber = l.VehicleLicenseNumber,
                    CitizenRegistryId = citizenId, // ? int ??? ????

                    PlateNumber = l.PlateNumber,
                    Brand = l.Brand,
                    Model = l.Model,
                    ManufactureYear = l.ManufactureYear,
                    IssueDate = l.IssueDate,
                    ExpiryDate = l.ExpiryDate,
                    Governorate = l.Governorate,
                    ChassisNumber = l.ChassisNumber,
                    EngineNumber = l.EngineNumber,
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