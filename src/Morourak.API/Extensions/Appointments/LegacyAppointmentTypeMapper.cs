using Morourak.Domain.Enums.Appointments;
using Morourak.Domain.Enums.Request;

namespace Morourak.API.Extensions.Appointments;

internal static class LegacyAppointmentTypeMapper
{
    public static bool TryMap(string? legacyServiceType, out AppointmentType appointmentType)
    {
        appointmentType = default;

        if (string.IsNullOrWhiteSpace(legacyServiceType))
            return false;

        var normalized = legacyServiceType.Trim();

        // Legacy Arabic labels used by older mobile clients.
        if (normalized.Equals("كشف طبي", StringComparison.OrdinalIgnoreCase))
        {
            appointmentType = AppointmentType.Medical;
            return true;
        }

        if (normalized.Equals("فحص فني", StringComparison.OrdinalIgnoreCase))
        {
            appointmentType = AppointmentType.Technical;
            return true;
        }

        if (normalized.Equals("اختبار قيادة", StringComparison.OrdinalIgnoreCase))
        {
            appointmentType = AppointmentType.Driving;
            return true;
        }

        // Service-type values (English enum keys) or Arabic display names that contain these keywords.
        if (normalized.Contains("مركبة", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals(nameof(ServiceType.VehicleLicenseIssue), StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals(nameof(ServiceType.VehicleLicenseRenewal), StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals(nameof(ServiceType.VehicleLicenseReplacementLost), StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals(nameof(ServiceType.VehicleLicenseReplacementDamaged), StringComparison.OrdinalIgnoreCase))
        {
            appointmentType = AppointmentType.Technical;
            return true;
        }

        if (normalized.Contains("قيادة", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals(nameof(ServiceType.DrivingLicenseIssue), StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals(nameof(ServiceType.DrivingLicenseRenewal), StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals(nameof(ServiceType.DrivingLicenseReplacementLost), StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals(nameof(ServiceType.DrivingLicenseReplacementDamaged), StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals(nameof(ServiceType.DrivingLicenseUpgrade), StringComparison.OrdinalIgnoreCase))
        {
            appointmentType = AppointmentType.Driving;
            return true;
        }

        return false;
    }
}

