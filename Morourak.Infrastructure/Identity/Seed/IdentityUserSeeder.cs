using Microsoft.AspNetCore.Identity;
using Morourak.Infrastructure.Identity;
using Morourak.Infrastructure.Identity.Constants;

namespace Morourak.Infrastructure.Identity.Seed;

public static class IdentityUserSeeder
{
    public static async Task SeedAsync(UserManager<ApplicationUser> userManager)
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
            role: AppIdentityConstants.Roles.Admin
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
            role: AppIdentityConstants.Roles.Inspector
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
            role: AppIdentityConstants.Roles.Examinator
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
            role: AppIdentityConstants.Roles.Doctor
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
        string role)
    {
        var existingUser = userManager.Users.FirstOrDefault(u => u.PhoneNumber == phoneNumber);
        if (existingUser != null)
            return;

        var user = new ApplicationUser
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
            throw new Exception($"Failed to create seeded user ({role}). Errors: {errors}");
        }

        await userManager.AddToRoleAsync(user, role);
    }
}