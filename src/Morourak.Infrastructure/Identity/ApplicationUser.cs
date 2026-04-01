using Microsoft.AspNetCore.Identity;
using System;

namespace Morourak.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser
    {
        // Personal Info
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        // Government Identifier
        public string NationalId { get; set; } = string.Empty; // 14 digits

        // Verification
        public bool IsVerified { get; set; } = false;

        // Account Status
        public bool IsActive { get; set; } = true;


        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; } = DateTime.UtcNow;
    }
}
