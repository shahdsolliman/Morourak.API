using Morourak.Domain.Entities;

namespace Morourak.Application.Interfaces.Services;

public interface IAdminSeedDataService
{
    Task<IEnumerable<object>> GetAllCitizensAsync();
    Task<IEnumerable<object>> GetAllVehicleLicensesAsync();
    Task<IEnumerable<object>> GetAllDrivingLicensesAsync();
}
