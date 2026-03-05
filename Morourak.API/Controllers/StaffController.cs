using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Morourak.Application.Interfaces.Services;
using Morourak.Domain.Enums.Appointments;
using Morourak.Infrastructure.Identity.Constants;
using System;
using System.Security.Claims;
using AppEx = Morourak.Application.Exceptions;

namespace Morourak.API.Controllers
{
    [Authorize(Roles = $"{AppIdentityConstants.Roles.Inspector},{AppIdentityConstants.Roles.Examinator},{AppIdentityConstants.Roles.Doctor}")]
    [ApiController]
    [Route("api/staff/examinations")]
    public class StaffController : ControllerBase
    {
        private readonly IAppointmentService _service;

        private static readonly Dictionary<string, AppointmentType> RoleTypeMap = new()
        {
            { AppIdentityConstants.Roles.Inspector, AppointmentType.Technical },
            { AppIdentityConstants.Roles.Examinator, AppointmentType.Driving },
            { AppIdentityConstants.Roles.Doctor, AppointmentType.Medical }
        };

        public StaffController(IAppointmentService service)
        {
            _service = service;
        }

        // ================= Get Appointments For Logged-in Staff =================
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(role) || !RoleTypeMap.ContainsKey(role))
                throw new AppEx.ValidationException(
                    "You are not authorized to access staff examinations.",
                    "AUTHZ_ERROR"
                );
            role = role.ToUpperInvariant();


            var appointments = await _service.GetByRoleAsync(role, userId);

            return Ok(new
            {
                IsSuccess = true,
                Count = appointments.Count(),
                Data = appointments
            });
        }

        // ================= Submit Examination Result =================
        [HttpPost("submit")]
        public async Task<IActionResult> Submit([FromBody] SubmitResultDto dto)
        {
            if (dto == null)
                throw new AppEx.ValidationException("Request body is required.", "BODY_MISSING");

            var role = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(role) || !RoleTypeMap.ContainsKey(role))
                throw new AppEx.ValidationException(
                    "You are not authorized to submit examination results.",
                    "AUTHZ_ERROR"
                );

            // Role determines appointment type (cannot be overridden by client)
            var appointmentType = RoleTypeMap[role];

            var staffUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _service.UpdateStatusAsync(
                dto.ApplicationId,
                appointmentType,
                dto.Passed,
                dto.Notes,
                staffUserId
            );

            return Ok(new
            {
                IsSuccess = true,
                Message = dto.Passed ? "Examination marked as PASSED." : "Examination marked as FAILED.",
                ApplicationId = dto.ApplicationId
            });
        }
    }

    // ================= DTO =================
    public class SubmitResultDto
    {
        public int ApplicationId { get; set; }
        public bool Passed { get; set; }
        public string? Notes { get; set; }
    }
}