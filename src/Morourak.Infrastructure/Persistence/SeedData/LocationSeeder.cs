using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Morourak.Domain.Entities;

namespace Morourak.Infrastructure.Persistence.SeedData
{
    /// <summary>
    /// Seeds default locations after TrafficUnits are present.
    /// Idempotent and FK-safe via navigation properties.
    /// </summary>
    public static class LocationSeeder
    {
        private static readonly string[] Unit1Locations =
        {
            "Main Waiting Hall",
            "Vehicle Licensing Desk",
            "Investigation Desk"
        };

        private static readonly string[] Unit2Locations =
        {
            "Payment Treasury",
            "Technical Inspection Hall"
        };

        private static readonly string[] Unit3Locations =
        {
            "License Delivery Desk",
            "Traffic Prosecution Office"
        };

        public static async Task SeedAsync(PersistenceDbContext context, ILogger logger)
        {
            var trafficUnits = await context.TrafficUnits
                .Include(t => t.Locations)
                .OrderBy(t => t.Id)
                .Take(3)
                .ToListAsync();

            if (trafficUnits.Count < 3)
            {
                logger.LogWarning(
                    "Skipping location seed because fewer than 3 TrafficUnits exist (found {Count}).",
                    trafficUnits.Count);
                return;
            }

            var now = DateTime.UtcNow;
            var addedCount = 0;

            addedCount += AddMissingLocations(trafficUnits[0], Unit1Locations, now);
            addedCount += AddMissingLocations(trafficUnits[1], Unit2Locations, now);
            addedCount += AddMissingLocations(trafficUnits[2], Unit3Locations, now);

            if (addedCount == 0)
            {
                logger.LogInformation("Locations already seeded; no new records.");
                return;
            }

            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} locations.", addedCount);
        }

        private static int AddMissingLocations(TrafficUnit trafficUnit, IEnumerable<string> names, DateTime createdAt)
        {
            var count = 0;

            foreach (var name in names)
            {
                if (trafficUnit.Locations.Any(l => l.Name == name))
                    continue;

                trafficUnit.Locations.Add(new Location
                {
                    Name = name,
                    CreatedAt = createdAt
                });

                count++;
            }

            return count;
        }
    }
}
