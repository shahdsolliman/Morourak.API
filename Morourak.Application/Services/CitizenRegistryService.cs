using Morourak.Application.Interfaces;
using Morourak.Application.Interfaces.Services;
using Morourak.Domain.Entities;

namespace Morourak.Application.Services
{
    public class CitizenRegistryService : ICitizenRegistryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CitizenRegistryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(bool IsValid, string Message)> ValidateAsync(
            string nationalId,
            string mobileNumber)
        {
            var citizen = (await _unitOfWork
                .Repository<CitizenRegistry>()
                .FindAsync(c => c.NationalId == nationalId))
                .FirstOrDefault();

            if (citizen == null)
                return (false, "National ID does not exist in government records.");

            if (citizen.MobileNumber != mobileNumber)
                return (false, "Mobile number does not match the provided National ID.");

            return (true, "Citizen validation successful.");
        }

        public async Task<int?> GetCitizenIdByNationalIdAsync(string nationalId)
        {
            var citizen = (await _unitOfWork
                .Repository<CitizenRegistry>()
                .FindAsync(c => c.NationalId == nationalId))
                .FirstOrDefault();

            return citizen?.Id;
        }
    }
}