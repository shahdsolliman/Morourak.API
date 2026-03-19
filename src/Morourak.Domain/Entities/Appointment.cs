using Morourak.Domain.Common;
using Morourak.Domain.Enums.Appointments;

namespace Morourak.Domain.Entities
{
    public class Appointment : BaseEntity<int>
    {
        public int ApplicationId { get; set; }

        // Citizen identifier (National ID)
        public string CitizenNationalId { get; set; } = null!;

        public AppointmentType Type { get; set; }

        public DateOnly Date { get; set; }

        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        public string RequestNumber { get; set; } = null!;

        // Location: stored at booking time so it never relies on cache
        public int GovernorateId { get; set; }
        public int TrafficUnitId { get; set; }

        // Navigation properties
        public virtual Governorate? Governorate { get; set; }
        public virtual TrafficUnit? TrafficUnit { get; set; }

        
   
    }
}
