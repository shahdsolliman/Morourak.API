using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Morourak.Application.DTOs.Appointments;
using Morourak.Application.Interfaces.Services;
using Morourak.Domain.Enums.Appointments;
using Morourak.Infrastructure.Identity.Seed;
using System.Security.Claims;

[Authorize(Roles = $"{IdentityRoles.Inspector},{IdentityRoles.Officer}")]
[ApiController]
[Route("api/staff/examinations")]
public class StaffController : ControllerBase
{
    private readonly IAppointmentService _service;

    private static readonly Dictionary<string, AppointmentType> RoleTypeMap = new()
    {
        { IdentityRoles.Inspector, AppointmentType.Technical },
        { IdentityRoles.Officer, AppointmentType.Driving }
    };

    public StaffController(IAppointmentService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var roleClaim = User.FindFirst(ClaimTypes.Role);
        if (roleClaim == null)
            return Unauthorized(new { message = "User role is missing." });

        if (!RoleTypeMap.TryGetValue(roleClaim.Value, out var type))
            return Forbid();

        var appointments = await _service.GetAppointmentsByTypeAsync(type);

        return Ok(appointments ?? new List<AppointmentDto>());
    }

    [HttpPut("{requestNumber}/status")]
    public async Task<IActionResult> UpdateStatus(string requestNumber, [FromBody] UpdateExaminationStatusDto dto)
    {
        if (dto == null) return BadRequest(new { message = "Request body is required." });
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        try
        {
            await _service.UpdateStatusAsync(requestNumber, dto.NewStatus);
            return Ok(new { message = "Appointment status updated successfully.", requestNumber, newStatus = dto.NewStatus.ToString() });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An unexpected error occurred while updating appointment status." });
        }
    }
}