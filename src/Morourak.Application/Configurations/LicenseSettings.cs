using Morourak.Domain.Enums.Driving;

namespace Morourak.Application.Configurations
{
    public class LicenseSettings
    {
        public const string SectionName = "LicenseSettings";
        
        public Dictionary<string, int> Durations { get; set; } = new();

        public int GetDurationYears(DrivingLicenseCategory category)
        {
            var key = category.ToString();
            return Durations.TryGetValue(key, out var duration) ? duration : 3; // Fallback to 3 if not configured
        }
    }
}
