using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MediatR;
using Morourak.Application.DTOs.Appointments;
using Morourak.Application.CQRS.Appointment.Commands.CreateAppointment;
using Morourak.Application.CQRS.Appointment.Queries.GetMyAppointments;
using Morourak.Application.Interfaces.Services;
using Morourak.Application.DTOs.Common;
using Morourak.Domain.Enums.Appointments;
using Morourak.API.Extensions.Appointments;
using Morourak.API.Formatting;
using Morourak.Infrastructure.Identity.Constants;
using System.Security.Claims;
using AppEx = Morourak.Application.Exceptions;

namespace Morourak.API.Controllers
{
    /// <summary>
    /// Controller for managing citizen appointments (Medical, Technical, Driving tests).
    /// </summary>
    [Route("api/v1/[controller]")]
    [Authorize]
    [Tags("Appointments")]
    public class AppointmentsController : BaseApiController
    {
        private readonly IMediator _mediator;
        private readonly IAppointmentQueryService _queryService;
        private readonly IAppointmentService _appointmentService;
        private readonly IGovernorateService _govService;
        private readonly IAppointmentArabicFormatter _formatter;

        public AppointmentsController(
            IMediator mediator,
            IAppointmentQueryService queryService,
            IAppointmentService appointmentService,
            IGovernorateService govService,
            IAppointmentArabicFormatter formatter)
        {
            _mediator = mediator;
            _queryService = queryService;
            _appointmentService = appointmentService;
            _govService = govService;
            _formatter = formatter;
        }

        /// <summary>
        /// Retrieves available time slots for a specific date, appointment type, and traffic unit.
        /// </summary>
        [HttpGet("available-slots")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableSlots(
        [FromQuery] DateOnly date,
        [FromQuery, BindRequired] AppointmentType type,
        [FromQuery] int trafficUnitId)
        {
            var slots = await _queryService.GetAvailableSlotsAsync(date, type, trafficUnitId);

            return Success(new
            {
                Date = date,
                Type = type,
                TrafficUnitId = trafficUnitId,
                Data = slots
            });
        }

        /// <summary>
        /// Confirms and books an appointment in a single operation.
        /// </summary>
        [HttpPost("book")]
        [Authorize(Roles = AppIdentityConstants.Roles.Citizen)]
        public async Task<IActionResult> Book([FromBody] ConfirmAppointmentRequestDto request)
        {
            var nationalId = User.FindFirstValue("NationalId");
            if (string.IsNullOrEmpty(nationalId))
                throw new AppEx.ValidationException("رقم الهوية غير موجود في رمز التحقق.", "AUTH_ERROR");

            var appointmentType = AppointmentTypeInputResolver.Resolve(request);

            // Resolve Arabic names to IDs
            var govId = await ResolveId(request.GovernorateId, (name) => _govService.ResolveGovernorateIdByNameAsync(name), "محافظة");
            var unitId = await ResolveId(request.TrafficUnitId, (name) => _govService.ResolveTrafficUnitIdByNameAsync(name, govId), "وحدة مرور");

            var result = await _mediator.Send(new CreateAppointmentCommand(
                NationalId: nationalId,
                AppointmentType: appointmentType,
                Date: request.Date,
                Time: request.Time,
                GovernorateId: govId,
                TrafficUnitId: unitId,
                RequestNumber: request.RequestNumber));

            result = _formatter.FormatBookingConfirmation(result);

            return Success(result, "تم حجز الموعد بنجاح");
        }

        /// <summary>
        /// Retrieves all appointments booked by the currently authenticated user (Citizen or Staff).
        /// </summary>
        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> MyAppointments([FromQuery] PaginationParams pagination, [FromQuery] DateOnly? date)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (User.IsInRole(AppIdentityConstants.Roles.Citizen))
            {
                var nationalId = User.FindFirstValue("NationalId");
                if (string.IsNullOrEmpty(nationalId))
                    throw new AppEx.ValidationException("رقم الهوية غير موجود في رمز التحقق.", "AUTH_ERROR");

                var result = await _mediator.Send(new GetMyAppointmentsQuery(nationalId, pagination));
                var formattedItems = result.Items.Select(x => _formatter.FormatAppointmentSummary(x)).ToList();
                result.Items = formattedItems;
                return SuccessPaginated(result.Items, result.PageNumber, result.PageSize, result.TotalCount, "Success");
            }
            
            if (User.IsInRole(AppIdentityConstants.Roles.Admin) || 
                User.IsInRole(AppIdentityConstants.Roles.Doctor) || 
                User.IsInRole(AppIdentityConstants.Roles.Examinator) || 
                User.IsInRole(AppIdentityConstants.Roles.Inspector))
            {
                // We need the raw role string for the query service mapping
                var roleString = User.FindFirstValue(ClaimTypes.Role);
                var appointments = await _queryService.GetByRoleAsync(roleString!, userId, date);
                return Success(appointments);
            }

            return Success(Enumerable.Empty<AppointmentDto>());
        }

        /// <summary>
        /// Updates the status of an appointment (Staff only).
        /// </summary>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = $"{AppIdentityConstants.Roles.Admin},{AppIdentityConstants.Roles.Doctor},{AppIdentityConstants.Roles.Examinator},{AppIdentityConstants.Roles.Inspector}")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);

            var appointment = await _queryService.GetByIdAsync(id);
            if (appointment == null) return NotFound(new { Message = "الموعد غير موجود" });

            // RBAC: Staff can only update their own assigned appointments (or unassigned ones of their specialty). 
            // Admin can update anything.
            if (role != AppIdentityConstants.Roles.Admin)
            {
                if (!string.IsNullOrEmpty(appointment.AssignedToUserId) && appointment.AssignedToUserId != userId)
                {
                    return StatusCode(403, new { Message = "غير مسموح لك بتحديث حالة هذا الموعد لأنه معين لموظف آخر." });
                }
            }

            await _appointmentService.UpdateStatusAsync(id, request.Status, request.Notes, userId!);
            return Success((object?)null, "تم تحديث حالة الموعد بنجاح");
        }

        /// <summary>
        /// Marks an appointment as 'In Progress' (Staff only).
        /// </summary>
        [HttpPost("{id}/start")]
        [Authorize(Roles = $"{AppIdentityConstants.Roles.Doctor},{AppIdentityConstants.Roles.Examinator},{AppIdentityConstants.Roles.Inspector}")]
        public async Task<IActionResult> StartAppointment(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);

            var appointment = await _queryService.GetByIdAsync(id);
            if (appointment == null) return NotFound(new { Message = "الموعد غير موجود" });

            if (!string.IsNullOrEmpty(appointment.AssignedToUserId) && appointment.AssignedToUserId != userId)
            {
                return StatusCode(403, new { Message = "غير مسموح لك ببدء هذا الموعد لأنه معين لموظف آخر." });
            }

            await _appointmentService.UpdateStatusAsync(id, AppointmentStatus.InProgress, "Started", userId!);
            return Success((object?)null, "بدأ الفحص الآن");
        }


        /// <summary>
        /// Retrieves appointment data fully in Arabic.
        /// </summary>
        /// <param name="id">Appointment ID.</param>
        /// <returns>Appointment and citizen details in Arabic.</returns>

        [HttpGet("appointment/{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Obsolete("Use the canonical GET 'api/v1/Appointments/my' endpoint. This route will be removed in a future version.")]
        public async Task<IActionResult> GetAppointmentArabic(int id)
        {
            var nationalId = User.FindFirstValue("NationalId");
            if (string.IsNullOrEmpty(nationalId))
                throw new AppEx.ValidationException("رقم الهوية غير موجود في رمز التحقق.", "AUTH_ERROR");

            var result = await _mediator.Send(new GetMyAppointmentDetailsQuery(id, nationalId));
            var formatted = result == null ? null : _formatter.FormatAppointmentDetails(result);

            return Success(formatted);
        }

        private async Task<int> ResolveId(object? idOrName, Func<string?, Task<int?>> resolver, string fieldName)
        {
            if (idOrName == null) throw new AppEx.ValidationException($"المجال '{fieldName}' مطلوب.", "REQUIRED_FIELD");

            string? inputAsString;
            if (idOrName is System.Text.Json.JsonElement ele)
            {
                if (ele.ValueKind == System.Text.Json.JsonValueKind.Number && ele.TryGetInt32(out var idNum)) return idNum;
                inputAsString = ele.GetString();
            }
            else
            {
                inputAsString = idOrName?.ToString();
            }

            if (string.IsNullOrWhiteSpace(inputAsString))
                throw new AppEx.ValidationException($"المجال '{fieldName}' مطلوب.", "REQUIRED_FIELD");

            if (int.TryParse(inputAsString, out var idParsed)) return idParsed;

            var resolvedId = await resolver(inputAsString);
            if (resolvedId.HasValue) return resolvedId.Value;

            throw new AppEx.ValidationException($"الاسم '{inputAsString}' غير موجود لـ {fieldName}.", "INVALID_NAME");
        }

        /// <summary>
        /// Assigns a staff member to an appointment (Admin only).
        /// </summary>
        [HttpPatch("{id}/assign")]
        [Authorize(Roles = AppIdentityConstants.Roles.Admin)]
        public async Task<IActionResult> AssignStaff(int id, [FromBody] AssignStaffRequestDto request)
        {
            await _appointmentService.AssignStaffAsync(id, request.StaffId, request.StaffName);
            return Success((object?)null, "تم تعيين الموظف بنجاح");
        }
    }

    public class AssignStaffRequestDto
    {
        public string StaffId { get; set; } = string.Empty;
        public string StaffName { get; set; } = string.Empty;
    }
}
