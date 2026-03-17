using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Morourak.API.Common;
using Morourak.Application.DTOs.Appointments;
using Morourak.Application.DTOs.Appointments.Arabic;
using AppEx = Morourak.Application.Exceptions;
using Morourak.Application.Interfaces.Services;
using Morourak.Domain.Enums.Appointments;
using Morourak.Infrastructure.Identity.Constants;
using System.Security.Claims;
using System.Globalization;

namespace Morourak.API.Controllers
{
    /// <summary>
    /// Controller for managing citizen appointments (Medical, Technical, Driving tests).
    /// </summary>
    [ApiController]
    [Route("api/appointments")]
    [Authorize]
    [Tags("Appointments")]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _service;

        public AppointmentsController(IAppointmentService service)
        {
            _service = service;
        }

        #region Helpers

        private static bool TryParseArabicDate(string value, out DateOnly date)
        {
            return DateOnly.TryParseExact(value, "d MMMM yyyy", new CultureInfo("ar-EG"), DateTimeStyles.None, out date);
        }

        private static bool TryParseArabicTime(string value, out TimeOnly time)
        {
            var normalized = value
                .Replace("صباحاً", "AM", StringComparison.Ordinal)
                .Replace("مساءً", "PM", StringComparison.Ordinal)
                .Trim();

            return TimeOnly.TryParseExact(normalized, "hh:mm tt", CultureInfo.GetCultureInfo("en-US"), DateTimeStyles.None, out time);
        }

        private موعدDto MapToBookedArabic(BookingConfirmationDto c)
        {
            if (!TryParseArabicDate(c.Appointment.DateFormatted, out var date))
                date = DateOnly.FromDateTime(DateTime.Today);

            if (!TryParseArabicTime(c.Appointment.TimeFormatted, out var time))
                time = default;

            return new موعدDto
            {
                رقم_الخدمة = c.ServiceRequest.RequestNumber,
                Id = c.Appointment.ApplicationId,
                التاريخ = date,
                وقت_البدء = time,
                الحالة = "محجوز",
                الرقم_القومي = c.ServiceRequest.CitizenNationalId,
                نوع_الموعد = c.Appointment.ServiceName
            };
        }

        private بيانات_موعدDto MapToDetailsArabic(AppointmentDto a)
        {
            return new بيانات_موعدDto
            {
                الرقم_القومي_للمواطن = a.CitizenNationalId,
                Id = a.ApplicationId,
                نوع_الموعد = a.Type switch
                {
                    AppointmentType.Medical => "كشف طبي",
                    AppointmentType.Technical => "فحص فني",
                    AppointmentType.Driving => "اختبار قيادة عملي",
                    _ => a.TypeName
                },
                اسم_الخدمة = a.ServiceName,
                التاريخ = a.Date,
                التاريخ_المنسق = a.DateFormatted,
                وقت_البدء = a.StartTime,
                الوقت_المنسق = a.TimeFormatted,
                الحالة = a.Status switch
                {
                    AppointmentStatus.Pending => "قيد الانتظار",
                    AppointmentStatus.Scheduled => "محجوز",
                    AppointmentStatus.Completed => "مكتمل",
                    AppointmentStatus.Cancelled => "ملغى",
                    AppointmentStatus.Passed => "ناجح",
                    AppointmentStatus.Available => "متاح",
                    AppointmentStatus.Failed => "راسب",
                    _ => a.Status.ToString()
                },
                تاريخ_الإنشاء = a.CreatedAt,
                رقم_الطلب_المرتبط = a.RequestNumberRelated,
                المحافظة = a.GovernorateName,
                وحدة_المرور = a.TrafficUnitName
            };
        }

        #endregion

        /// <summary>
        /// Retrieves available time slots for a specific date, appointment type, and traffic unit.
        /// </summary>
        [HttpGet("available-slots")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponseArabic), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseArabic), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAvailableSlots(
        [FromQuery] DateOnly date,
        [FromQuery] AppointmentType type,
        [FromQuery] int trafficUnitId)
        {
            var slots = await _service.GetAvailableSlotsAsync(date, type, trafficUnitId);

            return Ok(ApiResponseArabic.Success(new
            {
                Date = date,
                Type = type,
                TrafficUnitId = trafficUnitId,
                Data = slots
            }));
        }

        /// <summary>
        /// Confirms and books an appointment in a single operation.
        /// </summary>
        [HttpPost("book")]
        [Authorize(Roles = AppIdentityConstants.Roles.Citizen)]
        [ProducesResponseType(typeof(ApiResponseArabic), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseArabic), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Book([FromBody] ConfirmAppointmentRequestDto request)
        {
            var nationalId = User.FindFirstValue("NationalId");
            if (string.IsNullOrEmpty(nationalId))
                throw new AppEx.ValidationException("رقم الهوية غير موجود في رمز التحقق.", "AUTH_ERROR");

            var result = await _service.ConfirmBookingAsync(nationalId, request);

            return Ok(ApiResponseArabic.Success(MapToBookedArabic(result), "تم حجز الموعد بنجاح"));
        }

        /// <summary>
        /// Retrieves all appointments booked by the currently authenticated citizen.
        /// </summary>
        [HttpGet("my")]
        [Authorize(Roles = AppIdentityConstants.Roles.Citizen)]
        [ProducesResponseType(typeof(ApiResponseArabic), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseArabic), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> MyAppointments()
        {
            var nationalId = User.FindFirstValue("NationalId");
            if (string.IsNullOrEmpty(nationalId))
                throw new AppEx.ValidationException("رقم الهوية غير موجود في رمز التحقق.", "AUTH_ERROR");

            var result = await _service.GetMyAppointmentsAsync(nationalId);

            return Ok(ApiResponseArabic.Success(result.Select(MapToDetailsArabic)));
        }
    }
}
