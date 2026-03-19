using Microsoft.AspNetCore.Identity;
using Morourak.Infrastructure.Identity.Constants;

namespace Morourak.Infrastructure.Identity.Seed
{
    public static class RoleSeeder
    {
        public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
        {
            var roles = new[]
            {
                AppIdentityConstants.Roles.Citizen,
                AppIdentityConstants.Roles.Inspector,
                AppIdentityConstants.Roles.Examinator,
                AppIdentityConstants.Roles.Admin
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}
