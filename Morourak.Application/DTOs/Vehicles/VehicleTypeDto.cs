namespace Morourak.Application.DTOs.Vehicles
{
    /// <summary>
    /// Data transfer object for a vehicle type (e.g., Car, Motorcycle).
    /// </summary>
    public class VehicleTypeDto
    {
        /// <summary>Internal numeric value representing the vehicle type.</summary>
        public int Value { get; set; }

        /// <summary>Friendly name of the vehicle type.</summary>
        public string Name { get; set; } = null!;
    }
}
