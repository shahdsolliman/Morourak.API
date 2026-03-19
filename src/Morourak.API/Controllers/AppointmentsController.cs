using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Morourak.Application.DTOs.Appointments;
using Morourak.Application.CQRS.Appointment.Commands.CreateAppointment;
using Morourak.Application.CQRS.Appointment.Queries.GetMyAppointments;
using Morourak.Application.Interfaces.Services;
using Morourak.Application.DTOs.Common;
using Morourak.Domain.Enums.Appointments;
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
        private readonly IAppointmentArabicFormatter _formatter;

        public AppointmentsController(
            IMediator mediator,
            IAppointmentQueryService queryService,
            IAppointmentArabicFormatter formatter)
        {
            _mediator = mediator;
            _queryService = queryService;
            _formatter = formatter;
        }

        /// <summary>
        /// Retrieves available time slots for a specific date, appointment type, and traffic unit.
        /// </summary>
        [HttpGet("available-slots")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableSlots(
        [FromQuery] DateOnly date,
        [FromQuery] AppointmentType type,
        [FromQuery] int trafficUnitId)
        {
            var slots = await _queryService.GetAvailableSlotsAsync(date, type, trafficUnitId);

            return Ok(new
            {
                isSuccess = true,
                message = (string?)null,
                errorCode = (string?)null,
                details = new
                {
                    Date = date,
                    Type = type,
                    TrafficUnitId = trafficUnitId,
                    Data = slots
                }
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

            var result = await _mediator.Send(new CreateAppointmentCommand(
                NationalId: nationalId,
                ServiceType: request.ServiceType,
                Date: request.Date,
                Time: request.Time,
                GovernorateId: request.GovernorateId,
                TrafficUnitId: request.TrafficUnitId));

            result = _formatter.FormatBookingConfirmation(result);

            return Ok(new
            {
                isSuccess = true,
                message = "تم حجز الموعد بنجاح",
                errorCode = (string?)null,
                details = result
            });
        }

        /// <summary>
        /// Retrieves all appointments booked by the currently authenticated citizen.
        /// </summary>
        [HttpGet("my")]
        [Authorize(Roles = AppIdentityConstants.Roles.Citizen)]
        public async Task<IActionResult> MyAppointments([FromQuery] PaginationParams pagination)
        {
            var nationalId = User.FindFirstValue("NationalId");
            if (string.IsNullOrEmpty(nationalId))
                throw new AppEx.ValidationException("رقم الهوية غير موجود في رمز التحقق.", "AUTH_ERROR");

            var result = await _mediator.Send(new GetMyAppointmentsQuery(nationalId, pagination));

            var formattedItems = result
                .Items
                .Select(x => _formatter.FormatAppointmentSummary(x))
                .ToList();

            result.Items = formattedItems;

            return Ok(new
            {
                isSuccess = true,
                message = "Success",
                errorCode = (string?)null,
                data = result,
                details = result
            });
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

            return Ok(new
            {
                isSuccess = true,
                message = (string?)null,
                errorCode = (string?)null,
                details = formatted
            });
        }

    }
}
