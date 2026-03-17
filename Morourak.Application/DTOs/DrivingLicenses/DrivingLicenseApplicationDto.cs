using Morourak.Domain.Enums;
using Morourak.Domain.Enums.Driving;

namespace Morourak.Application.DTOs.DrivingLicenses
{
    /// <summary>
    /// Details of a driving license application.
    /// </summary>
    public class DrivingLicenseApplicationDto
    {
        /// <summary>
        /// Internal unique identifier for the application.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Requested license category.
        /// </summary>
        public string Category { get; set; } 

        /// <summary>
        /// Selected governorate for the application.
        /// </summary>
        public string Governorate { get; set; } = null!; 

        /// <summary>
        /// Selected traffic unit for the application.
        /// </summary>
        public string LicensingUnit { get; set; } = null!;

        /// <summary>
        /// Current status of the application (e.g., Pending, Approved).
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Public service request number for tracking.
        /// </summary>
        public string RequestNumber { get; set; }
    }
}