using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Morourak.Domain.Entities;

namespace Morourak.Infrastructure.Persistence.SeedData
{
    /// <summary>
    /// Seeds mock citizen registry data from JSON file.
    /// Idempotent: skips citizens that already exist by NationalId.
    /// </summary>
    public static class CitizenRegistrySeeder
    {
        public static async Task SeedAsync(PersistenceDbContext context, ILogger logger)
        {
            var filePath = Path.Combine(
                AppContext.BaseDirectory,
                "Persistence",
                "SeedData",
                "citizen-registry.json");

            if (!File.Exists(filePath))
            {
                logger.LogWarning("Citizen registry seed file not found at {Path}", filePath);
                return;
            }

            var jsonData = await File.ReadAllTextAsync(filePath);
            var citizens = JsonSerializer.Deserialize<List<CitizenRegistrySeedModel>>(jsonData);

            if (citizens == null || !citizens.Any())
            {
                logger.LogInformation("No citizen records in seed file.");
                return;
            }

            var existingNationalIds = await context.CitizenRegistries
                .Select(c => c.NationalId)
                .ToListAsync();

            var toAdd = citizens
                .Where(c => !existingNationalIds.Contains(c.NationalId))
                .Select(c => new CitizenRegistry
                {
                    NationalId = c.NationalId,
                    MobileNumber = c.MobileNumber,
                    NameAr = c.NameAr,
                    FatherFirstNameAr = c.FatherFirstNameAr
                })
                .ToList();

            if (toAdd.Count == 0)
            {
                logger.LogInformation("Citizen registry already seeded; no new records.");
                return;
            }

            await context.CitizenRegistries.AddRangeAsync(toAdd);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} citizen registry records.", toAdd.Count);
        }
    }
}
