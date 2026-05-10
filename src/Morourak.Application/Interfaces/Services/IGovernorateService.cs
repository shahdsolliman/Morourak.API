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

        /// <summary>إرجاع وحدات المرور التابعة لمحافظة معينة.</summary>
        Task<List<TrafficUnitDto>> GetTrafficUnitsByGovernorateAsync(int governorateId);

        /// <summary>محاولة الوصول لمُعرف المحافظة بالاسم</summary>
        Task<int?> ResolveGovernorateIdByNameAsync(string? name);

        /// <summary>محاولة الوصول لمُعرف وحدة المرور بالاسم</summary>
        Task<int?> ResolveTrafficUnitIdByNameAsync(string? name, int? governorateId = null);
    }
}
