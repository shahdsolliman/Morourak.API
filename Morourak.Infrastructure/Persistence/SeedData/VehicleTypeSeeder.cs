using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Morourak.Domain.Entities;
using System.Text.Json;

namespace Morourak.Infrastructure.Persistence.SeedData
{
    public static class VehicleTypeSeeder
    {
        public static async Task SeedAsync(PersistenceDbContext context, ILogger logger)
        {
            var filePath = Path.Combine(
                AppContext.BaseDirectory,
                "Persistence",
                "SeedData",
                "vehicle-types.json");

            if (!File.Exists(filePath))
            {
                logger.LogWarning("Vehicle types seed file not found at {Path}", filePath);
                return;
            }

            var jsonData = await File.ReadAllTextAsync(filePath);
            var types = JsonSerializer.Deserialize<List<VehicleTypeSeedModel>>(jsonData);

            if (types == null || !types.Any())
            {
                logger.LogInformation("No vehicle types in seed file.");
                return;
            }

            // Check if we need to re-seed (e.g. if Arabic names are missing)
            var needsReseed = await context.VehicleTypes.AnyAsync(v => string.IsNullOrEmpty(v.VehicleTypeAr));
            
            if (needsReseed)
            {
                logger.LogInformation("Re-seeding Vehicle Types to include Arabic translations...");
                
                // Clear existing records safely
                var existing = await context.VehicleTypes.ToListAsync();
                context.VehicleTypes.RemoveRange(existing);
                await context.SaveChangesAsync();
            }
            else
            {
                var count = await context.VehicleTypes.CountAsync();
                if (count > 0)
                {
                    logger.LogInformation("Vehicle types already seeded with Arabic data.");
                    return;
                }
            }

            var toAdd = new List<VehicleTypeEntity>();

            foreach (var t in types)
            {
                foreach (var brand in t.Brands)
                {
                    foreach (var model in brand.Models)
                    {
                        toAdd.Add(new VehicleTypeEntity
                        {
                            VehicleType = t.VehicleType,
                            VehicleTypeAr = t.VehicleTypeAr,
                            Brand = brand.Name,
                            BrandAr = brand.NameAr,
                            Model = model.Name,
                            ModelAr = model.NameAr
                        });
                    }
                }
            }

            await context.VehicleTypes.AddRangeAsync(toAdd);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} vehicle types with Arabic translations.", toAdd.Count);
        }
    }
}
