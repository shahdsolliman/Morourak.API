namespace Morourak.Application.DTOs.Appointments
{
    public class BookingConfirmationDto
    {
        public string Message { get; set; } = string.Empty;
        public string TrafficUnitName { get; set; } = string.Empty;
        public string TrafficUnitAddress { get; set; } = string.Empty;
        public string WorkingHours { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string BookingNumber { get; set; } = string.Empty; // Using string to match RequestNumber or int if preferred, business flow says number but RequestNumber is string
        public string RequestNumber { get; set; } = string.Empty;
    }
}
