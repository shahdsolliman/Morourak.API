using Morourak.Domain.Common;

namespace Morourak.Domain.Entities
{
    public class Governorate : BaseEntity<int>
    {
        public string Name { get; set; } = string.Empty; // الاسم بالعربي

        public ICollection<TrafficUnit> TrafficUnits { get; set; } = new List<TrafficUnit>();
    }
}
