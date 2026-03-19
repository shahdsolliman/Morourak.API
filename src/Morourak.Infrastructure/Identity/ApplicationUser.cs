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
        public bool IsVerified { get; set; } = false; // جديد: false عند التسجيل
        public string? VerificationCode { get; set; } // OTP
        public DateTime? VerificationCodeExpiry { get; set; } // انتهاء صلاحية OTP
        public int OtpAttempts { get; set; } = 0;

        // Account Status
        public bool IsActive { get; set; } = true;

        // Soft Delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}
