using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Morourak.Domain.Entities;
using Morourak.Domain.Enums;
using Morourak.Domain.Enums.Driving;

namespace Morourak.Infrastructure.Persistence.SeedData
{
    public static class DrivingLicenseSeeder
    {
        public static async Task SeedAsync(PersistenceDbContext context, ILogger logger)
        {
            var filePath = Path.Combine(
                AppContext.BaseDirectory,
                "Persistence",
                "SeedData",
                "driving-licenses.json");

            if (!File.Exists(filePath))
            {
                logger.LogWarning("Driving licenses seed file not found at {Path}", filePath);
                return;
            }

            var jsonData = await File.ReadAllTextAsync(filePath);
            var licenses = JsonSerializer.Deserialize<List<DrivingLicenseSeedModel>>(jsonData);

            if (licenses == null || !licenses.Any())
            {
                logger.LogInformation("No driving licenses in seed file.");
                return;
            }

            var existingNumbers = await context.DrivingLicenses
                .Select(d => d.LicenseNumber)
                .ToListAsync();

            var citizenByNationalId = await context.CitizenRegistries
                .ToDictionaryAsync(c => c.NationalId, c => c.Id);

            var toAdd = new List<DrivingLicense>();
            foreach (var l in licenses)
            {
                if (existingNumbers.Contains(l.LicenseNumber))
                    continue;

                if (!citizenByNationalId.TryGetValue(l.CitizenNationalId, out var citizenId))
                {
                    logger.LogWarning("Citizen with NationalId {NationalId} not found; skipping license {LicenseNumber}.",
                        l.CitizenNationalId, l.LicenseNumber);
                    continue;
                }

                toAdd.Add(new DrivingLicense
                {
                    LicenseNumber = l.LicenseNumber,
                    CitizenRegistryId = citizenId,
                    Category = Enum.Parse<DrivingLicenseCategory>(l.Category),
                    IssueDate = DateOnly.FromDateTime(l.IssueDate),
                    ExpiryDate = DateOnly.FromDateTime(l.ExpiryDate),
                    Governorate = l.Governorate,
                    LicensingUnit = l.LicensingUnit
                });
            }

            if (toAdd.Count == 0)
            {
                logger.LogInformation("Driving licenses already seeded; no new records.");
                return;
            }

            await context.DrivingLicenses.AddRangeAsync(toAdd);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} driving licenses.", toAdd.Count);
        }
    }
}
