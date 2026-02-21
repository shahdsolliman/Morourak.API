using Morourak.Domain.Enums.Appointments;
using Morourak.Infrastructure.Identity.Seed;

namespace Morourak.Infrastructure.Identity.Constants
{
    public static class RoleExaminationMap
    {
        public static readonly Dictionary<string, AppointmentType> Map =
            new()
            {
            { IdentityRoles.Inspector, AppointmentType.Technical },
            { IdentityRoles.Officer, AppointmentType.Driving }
            };
    }

}
