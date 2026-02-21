using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
            return Ok(new
            {
                Message = "You are authenticated",
                User = User.Identity!.Name
            });
        }

        // Citizen only
        [Authorize(Roles = "CITIZEN")]
        [HttpGet("citizen")]
        public IActionResult CitizenOnly()
        {
            return Ok("Citizen endpoint accessed");
        }

        // Officer only
        [Authorize(Roles = "OFFICER")]
        [HttpGet("officer")]
        public IActionResult OfficerOnly()
        {
            return Ok("Officer endpoint accessed");
        }


    }
}
