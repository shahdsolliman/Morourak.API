namespace Morourak.Application.DTOs.Governorates
{
    /// <summary>بيانات وحدة المرور المُعادة للفروند إند</summary>
    public class TrafficUnitDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Address { get; set; } // عنوان وحدة المرور
        public string? WorkingHours { get; set; } // مواعيد العمل
    }
}
