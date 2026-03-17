using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Morourak.API.Common;
using Morourak.Application.Interfaces.Services;
using Morourak.Domain.Enums.Appointments;
using Morourak.Infrastructure.Identity.Constants;
using System;
using System.Security.Claims;
using AppEx = Morourak.Application.Exceptions;

namespace Morourak.API.Controllers
{
    /// <summary>
    /// Controller for staff members (Inspectors, Examinators, Doctors) to manage appointments and results.
    /// </summary>
    [Authorize(Roles = $"{AppIdentityConstants.Roles.Inspector},{AppIdentityConstants.Roles.Examinator},{AppIdentityConstants.Roles.Doctor}")]
    [ApiController]
    [Route("api/v1/staff/examinations")]
    [Tags("Staff Operations")]
    public class StaffController : BaseApiController
    {
        private readonly IAppointmentService _service;
        private readonly IArabicDataService _arabicDataService;

        private static readonly Dictionary<string, AppointmentType> RoleTypeMap = new()
        {
            { AppIdentityConstants.Roles.Inspector, AppointmentType.Technical },
            { AppIdentityConstants.Roles.Examinator, AppointmentType.Driving },
            { AppIdentityConstants.Roles.Doctor, AppointmentType.Medical }
        };

        public StaffController(IAppointmentService service, IArabicDataService arabicDataService)
        {
            _service = service;
            _arabicDataService = arabicDataService;
        }

        /// <summary>
        /// Retrieves the list of appointments assigned to the logged-in staff member based on their role.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponseArabic), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseArabic), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Get()
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(role) || !RoleTypeMap.ContainsKey(role))
                throw new AppEx.ValidationException(
                    "غير مصرح لك بالوصول لهذه البيانات.",
                    "AUTHZ_ERROR"
                );
            role = role.ToUpperInvariant();

            var appointments = await _arabicDataService.GetArabicAppointmentsByRoleAsync(role, userId);

            return Ok(ApiResponseArabic.Success(appointments));
        }

        /// <summary>
        /// Submits the final examination or inspection result for a specific service request.
        /// </summary>
        [HttpPost("submit")]
        [ProducesResponseType(typeof(ApiResponseArabic), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseArabic), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Submit([FromBody] SubmitResultDto dto)
        {
            if (dto == null) 
            throw new AppEx.ValidationException("طلب الخدمة غير موجود.", "REQUEST_NOT_FOUND");

            var role = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(role) || !RoleTypeMap.ContainsKey(role))
                throw new AppEx.ValidationException(
                    "غير مصرح لك بتسليم هذه النتائج.",
                    "AUTHZ_ERROR"
                );

            var appointmentType = RoleTypeMap[role];
            var staffUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _service.UpdateStatusAsync(
                dto.RequestNumber,
                appointmentType,
                dto.Passed,
                dto.Notes,
                staffUserId
            );

            return Ok(ApiResponseArabic.Success(
                new { RequestNumber = dto.RequestNumber },
                dto.Passed ? "تم تسجيل نجاح الفحص." : "تم تسجيل رسوب الفحص."
            ));
        }
    }

    /// <summary>
    /// Data required to submit an examination or inspection result.
    /// </summary>
    public class SubmitResultDto
    {
        /// <summary>
        /// The unique request number associated with the appointment.
        /// </summary>
        /// <example>REQ-123456789</example>
        public string RequestNumber { get; set; } = string.Empty;

        /// <summary>
        /// Indicates whether the citizen passed the check.
        /// </summary>
        public bool Passed { get; set; }

        /// <summary>
        /// Staff notes or comments regarding the examination result.
        /// </summary>
        public string? Notes { get; set; }
    }
}
