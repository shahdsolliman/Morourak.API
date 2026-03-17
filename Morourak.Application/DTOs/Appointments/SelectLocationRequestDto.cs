namespace Morourak.Application.DTOs.Appointments
{
    /// <summary>
    /// Request DTO for selecting a location (Governorate and Traffic Unit).
    /// </summary>
    public class SelectLocationRequestDto
    {
        /// <summary>Selected governorate ID.</summary>
        public int GovernorateId { get; set; }

        /// <summary>Selected traffic unit ID.</summary>
        public int TrafficUnitId { get; set; }
    }
}
