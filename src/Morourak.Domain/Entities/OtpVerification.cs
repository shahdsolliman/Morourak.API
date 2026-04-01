using System;

namespace Morourak.Domain.Entities
{
    /// <summary>
    /// Generic entity for OTP verification (e.g., Password Reset).
    /// </summary>
    public class OtpVerification
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public string Identifier { get; set; } = null!; // MobileNumber or Email
        public string Code { get; set; } = null!;
        public DateTime Expiry { get; set; }
        public int Attempts { get; set; } = 0;
        public string Type { get; set; } = null!; // e.g. "ResetPassword"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
