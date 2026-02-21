namespace Morourak.Domain.Entities
{
    /// <summary>
    /// Represents a mock governmental citizen registry.
    /// Used ONLY for validation during registration.
    /// 
    /// IMPORTANT:
    /// - This is NOT an Identity user.
    /// - Data is seeded from JSON.
    /// - Simulates integration with a national civil registry.
    /// </summary>
    public class CitizenRegistry
    {
        /// <summary>
        /// Internal database primary key.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Citizen National ID (14 digits).
        /// Used as main identifier for validation.
        /// </summary>
        public string NationalId { get; set; } = null!;

        /// <summary>
        /// Official mobile number linked to National ID.
        /// Registration is allowed only if this matches.
        /// </summary>
        public string MobileNumber { get; set; } = null!;

        /// <summary>
        /// Citizen full name in Arabic.
        /// Used for display and demo purposes.
        /// </summary>
        public string NameAr { get; set; } = null!;

        /// <summary>
        /// Father's first name in Arabic.
        /// Often used for governmental identity verification.
        /// </summary>
        public string FatherFirstNameAr { get; set; } = null!;

        /// <summary>
        /// Navigation property: All vehicle licenses of this citizen.
        /// One-to-many relationship.
        /// </summary>
        public ICollection<VehicleLicense> VehicleLicenses { get; set; } = new List<VehicleLicense>();
    }
}