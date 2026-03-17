using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Morourak.API.Common;

namespace Morourak.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        // Any logged-in user
        [Authorize]
        [HttpGet("secure")]
        public IActionResult SecureEndpoint()
        {
            return Ok(ApiResponseArabic.Success(new
            {
                User = User.Identity!.Name
            }, "أنت مسجل الدخول"));
        }

        // Citizen only
        [Authorize(Roles = "CITIZEN")]
        [HttpGet("citizen")]
        public IActionResult CitizenOnly()
        {
            return Ok(ApiResponseArabic.Success(null, "تم الوصول لنقطة المواطن بنجاح"));
        }

        // Examinator only
        [Authorize(Roles = "EXAMINATOR")]
        [HttpGet("examinator")]
        public IActionResult ExaminatorOnly()
        {
            return Ok(ApiResponseArabic.Success(null, "تم الوصول لنقطة الفاحص بنجاح"));
        }
    }
}
