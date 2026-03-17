namespace Morourak.Application.DTOs.Vehicles
{
    /// <summary>
    /// Details of a vehicle license application.
    /// </summary>
    public class VehicleLicenseApplicationDto
    {
        /// <summary>
        /// Internal unique identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Type of vehicle for the application.
        /// </summary>
        public string VehicleType { get; set; } = null!;

        /// <summary>
        /// Vehicle brand.
        /// </summary>
        public string Brand { get; set; } = null!;

        /// <summary>
        /// Vehicle model.
        /// </summary>
        public string Model { get; set; } = null!;

        /// <summary>
        /// Current application status.
        /// </summary>
        public string Status { get; set; } = null!;

        /// <summary>
        /// Service request number for tracking.
        /// </summary>
        public string RequestNumber { get; set; } = null!;
    }
}
