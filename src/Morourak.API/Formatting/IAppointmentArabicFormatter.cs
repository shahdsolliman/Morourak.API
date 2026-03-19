using Morourak.Application.DTOs.Appointments;

namespace Morourak.API.Formatting;

public interface IAppointmentArabicFormatter
{
    BookingConfirmationDto FormatBookingConfirmation(BookingConfirmationDto dto);

    AppointmentSummaryDto FormatAppointmentSummary(AppointmentSummaryDto dto);
    AppointmentDetailsDto FormatAppointmentDetails(AppointmentDetailsDto dto);
}

