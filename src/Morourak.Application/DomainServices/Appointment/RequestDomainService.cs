using Morourak.Application.Interfaces;
using Morourak.Application.Interfaces.DomainServices;
using Morourak.Domain.Entities;
using Morourak.Domain.Enums.Appointments;
using Morourak.Domain.Enums.Request;

namespace Morourak.Application.DomainServices.Appointment;

public sealed class RequestDomainService : IRequestDomainService
{
    private readonly IUnitOfWork _unitOfWork;

    public RequestDomainService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceRequest?> FindPrimaryServiceRequestAsync(
        string nationalId,
        AppointmentType appointmentType,
        CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<ServiceRequest>();

        var primaryServiceTypes = GetPrimaryServiceTypesForAppointment(appointmentType);

        // Consider only active/non-cancelled requests.
        var relatedRequests = await repo.FindAsync(sr =>
            sr.CitizenNationalId == nationalId &&
            sr.ReferenceId > 0 &&
            primaryServiceTypes.Contains(sr.ServiceType) &&
            sr.Status != RequestStatus.Cancelled);

        // Pick the latest request with the expected prefix for that service.
        return relatedRequests
            .Where(sr =>
                sr.RequestNumber.StartsWith(
                    $"{GetRequestPrefix(sr.ServiceType)}-",
                    StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(sr => sr.SubmittedAt)
            .ThenByDescending(sr => sr.LastUpdatedAt ?? sr.SubmittedAt)
            .FirstOrDefault();
    }

    private static ServiceType[] GetPrimaryServiceTypesForAppointment(AppointmentType appointmentType)
    {
        return appointmentType switch
        {
            AppointmentType.Technical => [
                ServiceType.VehicleLicenseIssue,
                ServiceType.VehicleLicenseRenewal,
                ServiceType.VehicleLicenseReplacementLost,
                ServiceType.VehicleLicenseReplacementDamaged
            ],
            AppointmentType.Medical or AppointmentType.Driving => [
                ServiceType.DrivingLicenseIssue,
                ServiceType.DrivingLicenseRenewal,
                ServiceType.DrivingLicenseReplacementLost,
                ServiceType.DrivingLicenseReplacementDamaged,
                ServiceType.DrivingLicenseUpgrade
            ],
            _ => Array.Empty<ServiceType>()
        };
    }

    private static string GetRequestPrefix(ServiceType serviceType)
    {
        return serviceType switch
        {
            ServiceType.DrivingLicenseIssue => "DL",
            ServiceType.DrivingLicenseRenewal => "DR",
            ServiceType.VehicleLicenseIssue => "VL",
            ServiceType.VehicleLicenseRenewal => "VR",
            ServiceType.VehicleLicenseReplacementLost => "RPL",
            ServiceType.VehicleLicenseReplacementDamaged => "RPD",
            ServiceType.DrivingLicenseReplacementLost => "EL",
            ServiceType.DrivingLicenseReplacementDamaged => "ED",
            _ => "SR"
        };
    }
}

