using Microsoft.AspNetCore.Identity;
using System;

namespace Morourak.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser
    {
        // Personal Info
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;

        // Government Identifier
        public string NationalId { get; set; } = null!; // 14 digits

        // Verification
        public bool IsVerified { get; set; } = false; // جديد: false عند التسجيل
        public string? VerificationCode { get; set; } // OTP
        public DateTime? VerificationCodeExpiry { get; set; } // انتهاء صلاحية OTP

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
