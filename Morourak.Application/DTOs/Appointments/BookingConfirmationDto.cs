namespace Morourak.Application.DTOs.Appointments
{
    /// <summary>
    /// Response DTO containing confirmation details for both the appointment and the service request.
    /// </summary>
    public class BookingConfirmationDto
    {
        /// <summary>Detailed information about the scheduled appointment.</summary>
        public BookingAppointmentDto Appointment { get; set; } = new();

        /// <summary>Summary of the created service request.</summary>
        public BookingServiceRequestDto ServiceRequest { get; set; } = new();
    }

    /// <summary>
    /// Details about the booked appointment within the confirmation.
    /// </summary>
    public class BookingAppointmentDto
    {
        /// <summary>Success message or guidance for the citizen.</summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>Unique booking identifier.</summary>
        public string BookingNumber { get; set; } = string.Empty;

        /// <summary>Internal application ID.</summary>
        public int ApplicationId { get; set; }

        /// <summary>Public request number.</summary>
        public string RequestNumber { get; set; } = string.Empty;

        /// <summary>Name of the requested service.</summary>
        public string ServiceName { get; set; } = string.Empty;

        /// <summary>Scheduled date as string.</summary>
        public string Date { get; set; } = string.Empty;

        /// <summary>Scheduled time as string.</summary>
        public string Time { get; set; } = string.Empty;

        /// <summary>Formatted date for display.</summary>
        public string DateFormatted { get; set; } = string.Empty;

        /// <summary>Formatted time for display.</summary>
        public string TimeFormatted { get; set; } = string.Empty;

        /// <summary>Name of the traffic unit.</summary>
        public string TrafficUnitName { get; set; } = string.Empty;

        /// <summary>Physical address of the traffic unit.</summary>
        public string TrafficUnitAddress { get; set; } = string.Empty;

        /// <summary>Governorate name.</summary>
        public string GovernorateName { get; set; } = string.Empty;

        /// <summary>Traffic unit working hours.</summary>
        public string WorkingHours { get; set; } = string.Empty;

        /// <summary>ID of the assigned staff member.</summary>
        public string AssignedToUserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Service request summary within the booking confirmation.
    /// </summary>
    public class BookingServiceRequestDto
    {
        /// <summary>The public tracking number.</summary>
        public string RequestNumber { get; set; } = string.Empty;

        /// <summary>National ID of the citizen.</summary>
        public string CitizenNationalId { get; set; } = string.Empty;

        /// <summary>The type of service requested.</summary>
        public string ServiceType { get; set; } = string.Empty;

        /// <summary>Current request status.</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>Current payment status.</summary>
        public string PaymentStatus { get; set; } = string.Empty;

        /// <summary>Timestamp of submission.</summary>
        public string SubmittedAt { get; set; } = string.Empty;

        /// <summary>Timestamp of last update.</summary>
        public string LastUpdatedAt { get; set; } = string.Empty;
    }
}
