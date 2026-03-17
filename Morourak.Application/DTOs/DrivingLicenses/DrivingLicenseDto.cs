using Morourak.Domain.Enums;
using Morourak.Domain.Enums.Driving;

namespace Morourak.Application.DTOs.Licenses
{
    /// <summary>
    /// Represents a driving license issued to a citizen.
    /// </summary>
    public class DrivingLicenseDto
    {
        /// <summary>
        /// Unique license identification number.
        /// </summary>
        public string LicenseNumber { get; set; } = null!;

        /// <summary>
        /// The category of the license (e.g., Private, Professional).
        /// </summary>
        public string Category { get; set; } = default!;

        /// <summary>
        /// Current status of the license (e.g., Active, Expired, Suspended).
        /// </summary>
        public string Status { get; set; } = default!;

        /// <summary>
        /// The national ID of the license holder.
        /// </summary>
        public string CitizenNationalId { get; set; } = null!;

        /// <summary>
        /// The traffic unit that issued the license.
        /// </summary>
        public string LicensingUnit { get; set; } = null!;

        /// <summary>
        /// The governorate where the license was issued.
        /// </summary>
        public string Governorate { get; set; } = null!;

        /// <summary>
        /// The date the license was issued.
        /// </summary>
        public DateOnly IssueDate { get; set; }

        /// <summary>
        /// The date the license expires.
        /// </summary>
        public DateOnly ExpiryDate { get; set; }
    }
}