using Morourak.Domain.Enums.Appointments;

namespace Morourak.Infrastructure.Identity.Constants
{
    public static class RoleExaminationMap
    {
        public static readonly Dictionary<string, AppointmentType> Map =
            new()
            {
                { AppIdentityConstants.Roles.Inspector, AppointmentType.Technical },
                { AppIdentityConstants.Roles.Examinator, AppointmentType.Driving },
                { AppIdentityConstants.Roles.Doctor, AppointmentType.Medical }
            };
    }
}
