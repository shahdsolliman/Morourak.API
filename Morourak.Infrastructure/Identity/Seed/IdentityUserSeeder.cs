using Microsoft.AspNetCore.Identity;
using Morourak.Infrastructure.Identity.Constants;

namespace Morourak.Infrastructure.Identity.Seed;

public static class IdentityUserSeeder
{
    public static async Task SeedAsync(UserManager<ApplicationUser> userManager)
    {
        // Doctor
        await SeedUserAsync(
            userManager,
            phoneNumber: "01000000001",
            username: "doctor.demo",
            email: "doctor@morourak.com",
            firstName: "Demo",
            lastName: "Doctor",
            nationalId: "99900000000001",
            role: IdentityConstants.Roles.Doctor
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
            role: IdentityConstants.Roles.Inspector
        );

        // Officer
        await SeedUserAsync(
            userManager,
            phoneNumber: "01000000003",
            username: "officer.demo",
            email: "officer@morourak.com",
            firstName: "Demo",
            lastName: "Officer",
            nationalId: "99900000000003",
            role: IdentityConstants.Roles.Officer
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

        var result = await userManager.CreateAsync(user, IdentityConstants.DefaultDemoPassword);
        if (!result.Succeeded)
            throw new Exception($"Failed to create seeded user ({role})");

        await userManager.AddToRoleAsync(user, role);
    }
}