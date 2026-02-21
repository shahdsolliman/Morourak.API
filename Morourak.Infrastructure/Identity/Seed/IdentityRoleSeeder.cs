using Microsoft.AspNetCore.Identity;

namespace Morourak.Infrastructure.Identity.Seed
{
    public static class IdentityRoleSeeder
    {
        public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles =
            {
                IdentityRoles.Citizen,
                IdentityRoles.Doctor,
                IdentityRoles.Inspector,
                IdentityRoles.Officer
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
