using Morourak.Domain.Common;

namespace Morourak.Domain.Entities
{
    public class TrafficUnit : BaseEntity<int>
    {
        public string Name { get; set; } = string.Empty; 
        
        public string? Address { get; set; } 
        
        public string? WorkingHours { get; set; } 

        public int GovernorateId { get; set; }
        
        public virtual Governorate Governorate { get; set; } = null!;

        public virtual ICollection<Location> Locations { get; set; } = new List<Location>();
    }
}
