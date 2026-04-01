using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Morourak.Application.Interfaces.Services;
using Morourak.Infrastructure.Identity.Constants;

namespace Morourak.API.Controllers;

/// <summary>
/// Controller for administrators to retrieve seeded governmental data.
/// </summary>
[Authorize(Roles = AppIdentityConstants.Roles.Admin)]
[Route("api/v1/[controller]")]
[Tags("Admin Data Management")]
public class AdminSeedDataController : BaseApiController
{
    private readonly IAdminSeedDataService _adminSeedDataService;

    public AdminSeedDataController(IAdminSeedDataService adminSeedDataService)
    {
        _adminSeedDataService = adminSeedDataService;
    }

    /// <summary>
    /// Retrieves all seeded citizen registry records.
    /// </summary>
    [HttpGet("citizens")]
    public async Task<IActionResult> GetAllCitizens()
    {
        var citizens = await _adminSeedDataService.GetAllCitizensAsync();
        return Ok(new
        {
            isSuccess = true,
            message = "تم استرجاع بيانات المواطنين بنجاح.",
            details = citizens
        });
    }

    /// <summary>
    /// Retrieves all seeded vehicle license records.
    /// </summary>
    [HttpGet("vehicle-licenses")]
    public async Task<IActionResult> GetAllVehicleLicenses()
    {
        var licenses = await _adminSeedDataService.GetAllVehicleLicensesAsync();
        return Ok(new
        {
            isSuccess = true,
            message = "تم استرجاع بيانات رخص المركبات بنجاح.",
            details = licenses
        });
    }

    /// <summary>
    /// Retrieves all seeded driving license records.
    /// </summary>
    [HttpGet("driving-licenses")]
    public async Task<IActionResult> GetAllDrivingLicenses()
    {
        var licenses = await _adminSeedDataService.GetAllDrivingLicensesAsync();
        return Ok(new
        {
            isSuccess = true,
            message = "تم استرجاع بيانات رخص القيادة بنجاح.",
            details = licenses
        });
    }
}
