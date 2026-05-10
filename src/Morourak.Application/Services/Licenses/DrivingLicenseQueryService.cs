using Morourak.Application.DTOs.Licenses;
using Morourak.Application.Interfaces.Services;
using Morourak.Application.Interfaces;
using Morourak.Domain.Entities;
using AutoMapper;
using Morourak.Application.Exceptions;
using Morourak.Application.DTOs.DrivingLicenses;

namespace Morourak.Application.Services.Licenses
{
    public class DrivingLicenseQueryService : IDrivingLicenseQueryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DrivingLicenseQueryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<DrivingLicenseDto>> GetAllLicensesByCitizenAsync(string nationalId)
        {
            try
            {
                if (string.IsNullOrEmpty(nationalId))
                    return new List<DrivingLicenseDto>();

                var citizen = await GetCitizenAsync(nationalId);
                // Return empty if citizen doesn't exist to prevent 500 for admins
                if (citizen == null) return new List<DrivingLicenseDto>();

                var licenseEntities = await _unitOfWork.Repository<DrivingLicense>().FindAsync(l => l.CitizenRegistryId == citizen.Id);

                if (licenseEntities == null || !licenseEntities.Any()) 
                    return new List<DrivingLicenseDto>();

                var result = new List<DrivingLicenseDto>();
                foreach (var l in licenseEntities)
                {
                    try
                    {
                        var dto = _mapper.Map<DrivingLicenseDto>(l);
                        if (dto != null)
                        {
                            dto.Governorate = citizen.Governorate ?? string.Empty;
                            dto.LicensingUnit = citizen.LicensingUnit ?? string.Empty;
                            dto.CitizenNationalId = citizen.NationalId;
                            result.Add(dto);
                        }
                    }
                    catch
                    {
                        // Skip corrupted records if mapping fails
                        continue;
                    }
                }

                return result;
            }
            catch (ValidationException) 
            {
                // If citizen not found, return empty list instead of 400 for better "my-licenses" UX
                return new List<DrivingLicenseDto>();
            }
            catch
            {
                // Fallback for everything else
                return new List<DrivingLicenseDto>();
            }
        }

        private async Task<CitizenRegistry?> GetCitizenSafeAsync(string nationalId)
        {
            try { return await _unitOfWork.Repository<CitizenRegistry>().GetAsync(c => c.NationalId == nationalId); }
            catch { return null; }
        }

        public async Task<DrivingLicenseApplicationDto> GetApplicationByIdAsync(int applicationId, string nationalId)
        {
            var citizen = await GetCitizenAsync(nationalId);
            var application = await _unitOfWork.Repository<DrivingLicenseApplication>()
                .GetAsync(a => a.Id == applicationId && a.CitizenRegistryId == citizen.Id);

            if (application == null)
            {
                throw new Morourak.Application.Exceptions.ValidationException("الطلب غير موجود أو لا يخصك.", "APPLICATION_NOT_FOUND");
            }

            return _mapper.Map<DrivingLicenseApplicationDto>(application);
        }

        public async Task<ServiceRequest?> GetRequestByNumberAsync(string requestNumber)
        {
            return await _unitOfWork.Repository<ServiceRequest>().GetAsync(r => r.RequestNumber == requestNumber);
        }

        private async Task<CitizenRegistry> GetCitizenAsync(string nationalId)
        {
            var citizen = await _unitOfWork.Repository<CitizenRegistry>().GetAsync(c => c.NationalId == nationalId);
            if (citizen == null) throw new ValidationException("المواطن غير موجود.", "CITIZEN_NOT_FOUND");
            return citizen;
        }
    }
}
