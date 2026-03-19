using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Morourak.Application.Interfaces.Services;
using Morourak.Domain.Entities;

namespace Morourak.API.Controllers
{
    [Route("api/v1/[controller]")]
    public class CitizensController : BaseApiController
    {
        private readonly ICitizenService _citizenService;

        public CitizensController(ICitizenService citizenService)
        {
            _citizenService = citizenService;
        }

        /// <summary>
        /// Gets a citizen by internal ID (cached for 10 minutes).
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(CitizenRegistry), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            var citizen = await _citizenService.GetByIdAsync(id);
            if (citizen is null)
                return NotFound();

            return Ok(citizen);
        }
    }
}

