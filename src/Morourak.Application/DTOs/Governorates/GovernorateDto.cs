namespace Morourak.Application.DTOs.Governorates
{
    /// <summary>بيانات المحافظة المُعادة للفروند إند</summary>
    /// <summary>
    /// Data transfer object for governorate information.
    /// </summary>
    public class GovernorateDto
    {
        /// <summary>Unique identifier for the governorate.</summary>
        public int Id { get; set; }

        /// <summary>Name of the governorate.</summary>
        public string Name { get; set; } = null!;
    }
}
