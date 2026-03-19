namespace Morourak.Application.DTOs.Governorates
{
    /// <summary>بيانات وحدة المرور المُعادة للفروند إند</summary>
    /// <summary>
    /// Data transfer object for traffic unit information.
    /// </summary>
    public class TrafficUnitDto
    {
        /// <summary>Unique identifier for the traffic unit.</summary>
        public int Id { get; set; }

        /// <summary>Name of the traffic unit.</summary>
        public string Name { get; set; } = null!;

        /// <summary>Physical address of the traffic unit.</summary>
        public string? Address { get; set; }

        /// <summary>Working hours of the traffic unit.</summary>
        public string? WorkingHours { get; set; }
    }
}
