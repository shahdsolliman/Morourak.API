using Morourak.Domain.Common;
using Morourak.Domain.Enums.Appointments;

namespace Morourak.Domain.Entities
{
    public class Appointment : BaseEntity<int>
    {

        public int ApplicationId { get; set; }
        // Citizen identifier (from Identity)
        public string CitizenNationalId { get; set; } = null!;

        public AppointmentType Type { get; set; }

        // Day of appointment
        public DateOnly Date { get; set; }

        // Time slot
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        public AppointmentStatus Status { get; set; }
            = AppointmentStatus.Pending;


        public string RequestNumber { get; set; } = null!;




    }
}
