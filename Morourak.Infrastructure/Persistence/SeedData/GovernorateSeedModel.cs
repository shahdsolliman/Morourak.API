namespace Morourak.Infrastructure.Persistence.SeedData
{
    /// <summary>نموذج البذر — محافظة مع وحدات مرور تابعة لها</summary>
    public class GovernorateSeedModel
    {
        public string Name { get; set; } = null!;
        public List<TrafficUnitSeedModel> TrafficUnits { get; set; } = new();
    }

    public class TrafficUnitSeedModel
    {
        public string Name { get; set; } = null!;
        public string? Address { get; set; }
        public string? WorkingHours { get; set; }
    }
}
