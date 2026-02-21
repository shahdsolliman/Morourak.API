using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Morourak.Application.DTOs.Appointments;
using Morourak.Application.Interfaces.Services;
using Morourak.Domain.Enums.Appointments;
using System.Security.Claims;

[ApiController]
[Route("api/appointments")]
[Authorize(Roles = "CITIZEN")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _service;
    private readonly IApplicationValidationService _validationService;

    public AppointmentsController(
        IAppointmentService service,
        IApplicationValidationService validationService)
    {
        _service = service;
        _validationService = validationService;
    }

    // ================= Book Appointment =================
    [HttpPost("book")]
    public async Task<IActionResult> Book([FromBody] BookAppointmentRequestDto request)
    {
        var nationalId = User.FindFirstValue("NationalId");
        if (string.IsNullOrEmpty(nationalId))
            return Unauthorized();

        // Validate using request number
        var valid = await _validationService.ValidateApplicationAsync(
            nationalId,
            request.RequestNumber,
            request.Type);

        if (!valid.IsValid)
            return BadRequest(new { Message = valid.Message });

        if (valid.ApplicationId == null)
            return BadRequest(new { Message = "Application ID could not be resolved." });

        var bookedAppointment = await _service.BookAsync(
            nationalId,
            request.RequestNumber,
            valid.ApplicationId,
            request.Type,
            request.Date,
            request.StartTime
        );

        return Ok(bookedAppointment);
    }
    // ================= Get Available Slots =================
    [HttpGet("available-slots")]
    public async Task<IActionResult> GetAvailableSlots([FromQuery] DateOnly date, [FromQuery] AppointmentType type)
    {
        var slots = await _service.GetAvailableSlotsAsync(date, type);
        return Ok(slots);
    }

    // ================= Get My Appointments =================
    [HttpGet("my")]
    public async Task<IActionResult> MyAppointments()
    {
        var nationalId = User.FindFirstValue("NationalId");
        if (string.IsNullOrEmpty(nationalId)) return Unauthorized();

        var result = await _service.GetMyAppointmentsAsync(nationalId);
        return Ok(result);
    }
}