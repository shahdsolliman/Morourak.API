using Morourak.Application.Interfaces;
using Morourak.Application.Interfaces.Services;
using Morourak.Domain.Entities;

namespace Morourak.Application.Services;

public class AdminSeedDataService : IAdminSeedDataService
{
    private readonly IUnitOfWork _unitOfWork;

    public AdminSeedDataService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<CitizenRegistry>> GetAllCitizensAsync()
    {
        return await _unitOfWork.Repository<CitizenRegistry>()
            .GetAllAsync(c => c.VehicleLicenses, c => c.DrivingLicenses);
    }

    public async Task<IEnumerable<VehicleLicense>> GetAllVehicleLicensesAsync()
    {
        return await _unitOfWork.Repository<VehicleLicense>().GetAllAsync();
    }

    public async Task<IEnumerable<DrivingLicense>> GetAllDrivingLicensesAsync()
    {
        return await _unitOfWork.Repository<DrivingLicense>().GetAllAsync();
    }
}
