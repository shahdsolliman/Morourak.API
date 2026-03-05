using Microsoft.AspNetCore.Mvc;
using Morourak.Application.Interfaces.Services;

namespace Morourak.API.Controllers
{
    /// <summary>
    /// نقاط نهاية قوائم المحافظات ووحدات المرور — بدون مصادقة (بيانات مرجعية عامة).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class GovernoratesController : ControllerBase
    {
        private readonly IGovernorateService _governorateService;

        public GovernoratesController(IGovernorateService governorateService)
        {
            _governorateService = governorateService;
        }

        /// <summary>
        /// إرجاع جميع المحافظات — يُستخدم لملء القائمة المنسدلة في الفروند إند.
        /// GET /api/governorates
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetGovernoratesAsync()
        {
            var governorates = await _governorateService.GetAllGovernoratesAsync();
            return Ok(governorates);
        }

        /// <summary>
        /// إرجاع وحدات المرور التابعة لمحافظة محددة — يُستخدم لملء القائمة المنسدلة الثانية.
        /// GET /api/governorates/{governorateId}/traffic-units
        /// </summary>
        /// <param name="governorateId">معرّف المحافظة</param>
        [HttpGet("{governorateId}/traffic-units")]
        public async Task<IActionResult> GetTrafficUnitsAsync(int governorateId)
        {
            var units = await _governorateService.GetTrafficUnitsByGovernorateAsync(governorateId);
            return Ok(units);
        }
    }
}
