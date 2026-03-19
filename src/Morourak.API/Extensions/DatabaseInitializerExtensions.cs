using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Morourak.Infrastructure.Identity;
using Morourak.Infrastructure.Identity.Seed;
using Morourak.Infrastructure.Persistence;
using Morourak.Infrastructure.Persistence.SeedData;

namespace Morourak.API.Extensions
{
    public static class DatabaseInitializerExtensions
    {
        public static async Task<WebApplication> InitializeDatabasesAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateAsyncScope();
            var services = scope.ServiceProvider;

            var logger = services.GetRequiredService<ILogger<Program>>();

            try
            {
                // ===============================
                // Identity Database
                // ===============================
                var identityContext =
                    services.GetRequiredService<IdentityDbContext>();

                await identityContext.Database.MigrateAsync();

                var roleManager =
                    services.GetRequiredService<RoleManager<IdentityRole>>();

                var userManager =
                    services.GetRequiredService<UserManager<ApplicationUser>>();

                await IdentityRoleSeeder.SeedAsync(roleManager);
                await IdentityUserSeeder.SeedAsync(userManager);

                // ===============================
                // Persistence Database
                // ===============================
                var persistenceContext =
                    services.GetRequiredService<PersistenceDbContext>();

                await persistenceContext.Database.MigrateAsync();

                // ===== Seed Core Data (idempotent) =====
                logger.LogInformation("Seeding Governorates and Traffic Units...");
                await GovernorateSeeder.SeedAsync(persistenceContext, logger);

                logger.LogInformation("Seeding Locations...");
                await LocationSeeder.SeedAsync(persistenceContext, logger);

                logger.LogInformation("Seeding Vehicle Types...");
                await VehicleTypeSeeder.SeedAsync(persistenceContext, logger);

                logger.LogInformation("Seeding Citizen Registry...");
                await CitizenRegistrySeeder.SeedAsync(persistenceContext, logger);

                logger.LogInformation("Seeding Driving Licenses...");
                await DrivingLicenseSeeder.SeedAsync(persistenceContext, logger);

                logger.LogInformation("Seeding Vehicle Licenses...");
                await VehicleLicenseSeeder.SeedAsync(persistenceContext, logger);

                logger.LogInformation("Seeding Traffic Violations...");
                await TrafficViolationSeeder.SeedAsync(persistenceContext, logger);

                logger.LogInformation("Databases initialized successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while initializing the databases.");
            }

            return app;
        }
    }
}
