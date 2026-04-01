using System;

namespace Morourak.Domain.Entities
{
    /// <summary>
    /// Temporarily stores registration data until OTP verification is successful.
    /// This ensures unverified users are NOT persisted in the AspNetUsers table.
    /// </summary>
    public class PendingRegistration
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string PhoneNumber { get; set; } = null!; // Key for OTP lookup
        public string Email { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string NationalId { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;

        // OTP details
        public string? OtpCode { get; set; }
        public DateTime? OtpExpiry { get; set; }
        public int OtpAttempts { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
