namespace Morourak.Application.DTOs.Appointments
{
    /// <summary>
    /// Response DTO containing confirmation details for both the appointment and the service request.
    /// </summary>
    public class BookingConfirmationDto
    {
        public BookingAppointmentDto Appointment { get; set; } = new();

        public BookingServiceRequestDto ServiceRequest { get; set; } = new();
    }

    /// <summary>
    /// Details about the booked appointment within the confirmation.
    /// </summary>
    public class BookingAppointmentDto
    {
        public string Message { get; set; } = string.Empty;

        public string BookingNumber { get; set; } = string.Empty;

        public int ApplicationId { get; set; }

        public string RequestNumber { get; set; } = string.Empty;

        public string ServiceName { get; set; } = string.Empty;

        public string Date { get; set; } = string.Empty;

        public string Time { get; set; } = string.Empty;

        public string DateFormatted { get; set; } = string.Empty;

        public string TimeFormatted { get; set; } = string.Empty;

        public string TrafficUnitName { get; set; } = string.Empty;

        public string TrafficUnitAddress { get; set; } = string.Empty;

        public string GovernorateName { get; set; } = string.Empty;

        public string WorkingHours { get; set; } = string.Empty;

        public string AssignedToUserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Service request summary within the booking confirmation.
    /// </summary>
    public class BookingServiceRequestDto
    {
        public string RequestNumber { get; set; } = string.Empty;

        public string CitizenNationalId { get; set; } = string.Empty;

        public string ServiceType { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string PaymentStatus { get; set; } = string.Empty;

        public string SubmittedAt { get; set; } = string.Empty;

        public string LastUpdatedAt { get; set; } = string.Empty;
    }
}
