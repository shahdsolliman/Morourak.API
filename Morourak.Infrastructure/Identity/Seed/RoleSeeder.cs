using Microsoft.AspNetCore.Identity;

namespace Morourak.Infrastructure.Identity.Seed
{
    /// <summary>
    /// Seeds system roles into Identity database
    /// </summary>
    public static class RoleSeeder
    {
        public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
        {
            var roles = new[]
            {
                IdentityRoles.Citizen,
                IdentityRoles.Doctor,
                IdentityRoles.Inspector,
                IdentityRoles.Officer
            };

            foreach (var role in roles)
            {
                // Check if role already exists
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}
