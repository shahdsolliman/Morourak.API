using Morourak.Domain.Common;

namespace Morourak.Domain.Entities
{
    public class Location : BaseEntity<int>
    {
        public string Name { get; set; } = string.Empty; // اسم الموقع (مثلاً: نظام الشباك الواحد، الخزينة)
        
        public int TrafficUnitId { get; set; }
        public virtual TrafficUnit TrafficUnit { get; set; } = null!;
    }
}
