using Morourak.Application.DTOs.Governorates;

namespace Morourak.Application.Interfaces.Services
{
    /// <summary>
    /// خدمة استعلامات المحافظات ووحدات المرور.
    /// تُستخدم لتوفير بيانات قوائم الاختيار للفروند إند.
    /// </summary>
    public interface IGovernorateService
    {
        /// <summary>إرجاع جميع المحافظات بالأسماء العربية</summary>
        Task<List<GovernorateDto>> GetAllGovernoratesAsync();

        /// <summary>
        /// إرجاع وحدات المرور التابعة لمحافظة معينة.
        /// يرمي <see cref="Morourak.Application.Exceptions.ValidationException"/> إذا لم توجد المحافظة.
        /// </summary>
        Task<List<TrafficUnitDto>> GetTrafficUnitsByGovernorateAsync(int governorateId);
    }
}
