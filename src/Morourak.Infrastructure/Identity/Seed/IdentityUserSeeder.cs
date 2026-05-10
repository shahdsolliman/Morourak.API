using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Morourak.Infrastructure.Identity;
using Morourak.Infrastructure.Identity.Constants;

namespace Morourak.Infrastructure.Identity.Seed;

public static class IdentityUserSeeder
{
    public static async Task SeedAsync(UserManager<ApplicationUser> userManager, bool isDevelopment = false)
    {
        // Admin
        await SeedUserAsync(
            userManager,
            phoneNumber: "01000000000",
            username: "admin.demo",
            email: "admin@morourak.com",
            firstName: "System",
            lastName: "Admin",
            nationalId: "99900000000000",
            roles: AppIdentityConstants.Roles.Admin
        );


        // Inspector
        await SeedUserAsync(
            userManager,
            phoneNumber: "01000000002",
            username: "inspector.demo",
            email: "inspector@morourak.com",
            firstName: "Demo",
            lastName: "Inspector",
            nationalId: "99900000000002",
            roles: AppIdentityConstants.Roles.Inspector
        );

        // Examinator
        await SeedUserAsync(
            userManager,
            phoneNumber: "01000000010", // Changed slightly to avoid conflicts if needed, but original was 01000000003
            username: "examinator.demo",
            email: "examinator@morourak.com",
            firstName: "Demo",
            lastName: "Examinator",
            nationalId: "99900000000010",
            roles: AppIdentityConstants.Roles.Examinator
        );

        // Doctor
        await SeedUserAsync(
            userManager,
            phoneNumber: "01000000020",
            username: "doctor.demo",
            email: "doctor@morourak.com",
            firstName: "Demo",
            lastName: "Doctor",
            nationalId: "99900000000020",
            roles: AppIdentityConstants.Roles.Doctor
        );
        
      
            await SeedUserAsync(
                userManager,
                phoneNumber: "01099999999",
                username: "test.user",
                email: "test@morourak.com",
                firstName: "Omar",
                lastName: "Ahmed",
                nationalId: "29902012345678",
                roles: new[] { AppIdentityConstants.Roles.Tester, AppIdentityConstants.Roles.Citizen }
            );

            // New Test Account: Active Citizen
            await SeedUserAsync(
                userManager,
                phoneNumber: "01011111111",
                username: "active.tester",
                email: "active@morourak.com",
                firstName: "activeACC",
                lastName: "TEST",
                nationalId: "12345678901234",
                roles: new[] { AppIdentityConstants.Roles.Tester, AppIdentityConstants.Roles.Citizen }
            );

            // New Test Account: Expired Citizen
            await SeedUserAsync(
                userManager,
                phoneNumber: "01022222222",
                username: "expired.tester",
                email: "expired@morourak.com",
                firstName: "expiredACC",
                lastName: "TEST",
                nationalId: "98765432109876",
                roles: new[] { AppIdentityConstants.Roles.Tester, AppIdentityConstants.Roles.Citizen }
            );
        
    }

    private static async Task SeedUserAsync(
        UserManager<ApplicationUser> userManager,
        string phoneNumber,
        string username,
        string email,
        string firstName,
        string lastName,
        string nationalId,
        params string[] roles)
    {
        var user = await userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = username,
                Email = email,
                PhoneNumber = phoneNumber,
                FirstName = firstName,
                LastName = lastName,
                NationalId = nationalId,

                IsVerified = true,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };

            var result = await userManager.CreateAsync(user, AppIdentityConstants.DefaultDemoPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Failed to create seeded user ({string.Join(", ", roles)}). Errors: {errors}");
            }
        }

        foreach (var roleName in roles)
        {
            if (!await userManager.IsInRoleAsync(user, roleName))
            {
                await userManager.AddToRoleAsync(user, roleName);
            }
        }
    }
}