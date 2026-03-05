using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Morourak.Application.DTOs.Appointments;
using AppEx = Morourak.Application.Exceptions;
using Morourak.Application.Interfaces.Services;
using Morourak.Domain.Enums.Appointments;
using Morourak.Infrastructure.Identity.Constants;
using System.Security.Claims;

namespace Morourak.API.Controllers
{
    [ApiController]
    [Route("api/appointments")]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _service;

        public AppointmentsController(IAppointmentService service)
        {
            _service = service;
        }

        // =====================================================================
        // GET /api/appointments/available-slots
        // Public — no authentication required
        // Returns available time slots for a given date and appointment type.
        // Does NOT write to the database.
        // =====================================================================
        [HttpGet("available-slots")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableSlots(
        [FromQuery] DateOnly date,
        [FromQuery] AppointmentType type,
        [FromQuery] int trafficUnitId)
        {
            var slots = await _service.GetAvailableSlotsAsync(date, type, trafficUnitId);

            return Ok(new
            {
                IsSuccess = true,
                Date = date,
                Type = type,
                TrafficUnitId = trafficUnitId,
                Data = slots
            });
        }

        // =====================================================================
        // POST /api/appointments/book
        // Citizen only — creates ServiceRequest + Appointment in one transaction.
        // =====================================================================
        [HttpPost("book")]
        [Authorize(Roles = AppIdentityConstants.Roles.Citizen)]
        public async Task<IActionResult> Book([FromBody] ConfirmAppointmentRequestDto request)
        {
            var nationalId = User.FindFirstValue("NationalId");
            if (string.IsNullOrEmpty(nationalId))
                throw new AppEx.ValidationException("User not authenticated.", "AUTH_ERROR");

            var result = await _service.ConfirmBookingAsync(nationalId, request);

            return Ok(new
            {
                IsSuccess = true,
                Data = result
            });
        }

        // =====================================================================
        // GET /api/appointments/my
        // Citizen only — returns the authenticated citizen's appointments.
        // =====================================================================
        [HttpGet("my")]
        [Authorize(Roles = AppIdentityConstants.Roles.Citizen)]
        public async Task<IActionResult> MyAppointments()
        {
            var nationalId = User.FindFirstValue("NationalId");
            if (string.IsNullOrEmpty(nationalId))
                throw new AppEx.ValidationException("User not authenticated.", "AUTH_ERROR");

            var result = await _service.GetMyAppointmentsAsync(nationalId);

            return Ok(new
            {
                IsSuccess = true,
                Count = result.Count(),
                Data = result
            });
        }
    }
}