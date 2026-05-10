using Morourak.Application.Interfaces;
using Morourak.Application.Interfaces.Services;
using Morourak.Domain.Entities;

namespace Morourak.Application.Services
{
    public class AdminSeedDataService : IAdminSeedDataService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminSeedDataService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<object>> GetAllCitizensAsync()
        {
            var citizens = await _unitOfWork.Repository<CitizenRegistry>().GetAllAsync();
            
            // Flattening projection for consistent API response
            return citizens.Select(c => new
            {
                c.Id,
                c.NationalId,
                c.MobileNumber,
                c.FirstName,
                c.LastName,
                c.Governorate,
                c.LicensingUnit
            });
        }

        public async Task<IEnumerable<object>> GetAllDrivingLicensesAsync()
        {
            var licenses = await _unitOfWork.Repository<DrivingLicense>().GetAllAsync(l => l.Citizen);

            return licenses.Select(l => new
            {
                l.Id,
                l.LicenseNumber,
                l.IssueDate,
                l.ExpiryDate,
                l.CurrentStatus,
                Category = l.Category.ToString(),
                CitizenNationalId = l.Citizen?.NationalId ?? "---",
                CitizenName = l.Citizen != null ? $"{l.Citizen.FirstName} {l.Citizen.LastName}".Trim() : "---"
            });
        }

        public async Task<IEnumerable<object>> GetAllVehicleLicensesAsync()
        {
            var licenses = await _unitOfWork.Repository<VehicleLicense>().GetAllAsync(l => l.Citizen);

            return licenses.Select(l => new
            {
                l.Id,
                l.PlateNumber,
                l.ChassisNumber,
                l.EngineNumber,
                l.IssueDate,
                l.ExpiryDate,
                l.CurrentStatus,
                VehicleType = l.VehicleType.ToString(),
                CitizenNationalId = l.Citizen?.NationalId ?? "---",
                CitizenName = l.Citizen != null ? $"{l.Citizen.FirstName} {l.Citizen.LastName}".Trim() : "---"
            });
        }
    }
}
