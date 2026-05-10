using Morourak.Application.DTOs.Governorates;
using Morourak.Application.Exceptions;
using Morourak.Application.Interfaces;
using Morourak.Application.Interfaces.Services;
using Morourak.Domain.Entities;
using AutoMapper;
using AppEx = Morourak.Application.Exceptions;

namespace Morourak.Application.Services
{
    /// <summary>
    /// خدمة استعلام المحافظات ووحدات المرور — تُستخدم لتوفير بيانات قوائم الاختيار.
    /// </summary>
    public class GovernorateService : IGovernorateService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GovernorateService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <inheritdoc/>
        public async Task<List<GovernorateDto>> GetAllGovernoratesAsync()
        {
            var governorates = await _unitOfWork.Repository<Governorate>().GetAllAsync();

            return governorates
                .OrderBy(g => g.Id)
                .Select(g => _mapper.Map<GovernorateDto>(g))
                .ToList();
        }

        public async Task<int?> ResolveGovernorateIdByNameAsync(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;

            var normalized = name.Trim()
                .Replace("محافظة ", "", StringComparison.OrdinalIgnoreCase)
                .Replace(" المحافظة", "", StringComparison.OrdinalIgnoreCase);

            var governorate = await _unitOfWork.Repository<Governorate>()
                .GetAsync(g => g.Name.Contains(normalized) || normalized.Contains(g.Name));
                
            return governorate?.Id;
        }

        public async Task<int?> ResolveTrafficUnitIdByNameAsync(string? name, int? governorateId = null)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;

            var normalized = name.Trim()
                .Replace("مرور ", "", StringComparison.OrdinalIgnoreCase)
                .Replace(" وحدة", "", StringComparison.OrdinalIgnoreCase);

            var units = await _unitOfWork.Repository<TrafficUnit>()
                .FindAsync(t => (governorateId == null || t.GovernorateId == governorateId.Value) && 
                                (t.Name.Contains(normalized) || normalized.Contains(t.Name)));

            return units.FirstOrDefault()?.Id;
        }

        /// <inheritdoc/>
        public async Task<List<TrafficUnitDto>> GetTrafficUnitsByGovernorateAsync(int governorateId)
        {
            // التحقق من وجود المحافظة
            var governorate = await _unitOfWork.Repository<Governorate>()
                .GetByIdAsync(governorateId);

            if (governorate == null)
                throw new AppEx.ValidationException(
                    "اختر محافظة صحيحة.",
                    "INVALID_GOVERNORATE");

            // استرجاع وحدات المرور التابعة لهذه المحافظة
            var units = await _unitOfWork.Repository<TrafficUnit>()
                .FindAsync(t => t.GovernorateId == governorateId);

            if (!units.Any())
                throw new AppEx.ValidationException(
                    "لا توجد وحدات مرور مسجّلة لهذه المحافظة.",
                    "NO_TRAFFIC_UNITS");

            return units
                .OrderBy(t => t.Id)
                .Select(t => _mapper.Map<TrafficUnitDto>(t))
                .ToList();
        }
    }
}
