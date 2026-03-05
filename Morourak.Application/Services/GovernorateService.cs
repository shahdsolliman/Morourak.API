using Morourak.Application.DTOs.Governorates;
using Morourak.Application.Exceptions;
using Morourak.Application.Interfaces;
using Morourak.Application.Interfaces.Services;
using Morourak.Domain.Entities;
using AppEx = Morourak.Application.Exceptions;

namespace Morourak.Application.Services
{
    /// <summary>
    /// خدمة استعلام المحافظات ووحدات المرور — تُستخدم لتوفير بيانات قوائم الاختيار.
    /// </summary>
    public class GovernorateService : IGovernorateService
    {
        private readonly IUnitOfWork _unitOfWork;

        public GovernorateService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <inheritdoc/>
        public async Task<List<GovernorateDto>> GetAllGovernoratesAsync()
        {
            var governorates = await _unitOfWork.Repository<Governorate>().GetAllAsync();

            return governorates
                .OrderBy(g => g.Id)
                .Select(g => new GovernorateDto
                {
                    Id = g.Id,
                    Name = g.Name
                })
                .ToList();
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
                .Select(t => new TrafficUnitDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Address = t.Address,
                    WorkingHours = t.WorkingHours
                })
                .ToList();
        }
    }
}
