using Morourak.Domain.Entities;

namespace Morourak.Application.Interfaces.Services;

public interface IAdminSeedDataService
{
    Task<IEnumerable<CitizenRegistry>> GetAllCitizensAsync();
    Task<IEnumerable<VehicleLicense>> GetAllVehicleLicensesAsync();
    Task<IEnumerable<DrivingLicense>> GetAllDrivingLicensesAsync();
}
