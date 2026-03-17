using System.Text.Json.Serialization;

namespace Morourak.Application.DTOs.Appointments
{
    /// <summary>
    /// Response DTO containing confirmation details for both the appointment and the service request.
    /// </summary>
    public class BookingConfirmationDto
    {
        [JsonPropertyName("الموعد")]
        public BookingAppointmentDto Appointment { get; set; } = new();

        [JsonPropertyName("طلب_الخدمة")]
        public BookingServiceRequestDto ServiceRequest { get; set; } = new();
    }

    /// <summary>
    /// Details about the booked appointment within the confirmation.
    /// </summary>
    public class BookingAppointmentDto
    {
        [JsonPropertyName("الرسالة")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("رقم_الحجز")]
        public string BookingNumber { get; set; } = string.Empty;

        [JsonPropertyName("معرف_الطلب")]
        public int ApplicationId { get; set; }

        [JsonPropertyName("رقم_الطلب")]
        public string RequestNumber { get; set; } = string.Empty;

        [JsonPropertyName("اسم_الخدمة")]
        public string ServiceName { get; set; } = string.Empty;

        [JsonPropertyName("التاريخ")]
        public string Date { get; set; } = string.Empty;

        [JsonPropertyName("الوقت")]
        public string Time { get; set; } = string.Empty;

        [JsonPropertyName("التاريخ_منسق")]
        public string DateFormatted { get; set; } = string.Empty;

        [JsonPropertyName("الوقت_منسق")]
        public string TimeFormatted { get; set; } = string.Empty;

        [JsonPropertyName("اسم_وحدة_المرور")]
        public string TrafficUnitName { get; set; } = string.Empty;

        [JsonPropertyName("عنوان_وحدة_المرور")]
        public string TrafficUnitAddress { get; set; } = string.Empty;

        [JsonPropertyName("اسم_المحافظة")]
        public string GovernorateName { get; set; } = string.Empty;

        [JsonPropertyName("ساعات_العمل")]
        public string WorkingHours { get; set; } = string.Empty;

        [JsonPropertyName("معرف_الموظف_المسؤول")]
        public string AssignedToUserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Service request summary within the booking confirmation.
    /// </summary>
    public class BookingServiceRequestDto
    {
        [JsonPropertyName("رقم_الطلب")]
        public string RequestNumber { get; set; } = string.Empty;

        [JsonPropertyName("الرق_القومي_للمواطن")]
        public string CitizenNationalId { get; set; } = string.Empty;

        [JsonPropertyName("نوع_الخدمة")]
        public string ServiceType { get; set; } = string.Empty;

        [JsonPropertyName("الحالة")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("حالة_الدفع")]
        public string PaymentStatus { get; set; } = string.Empty;

        [JsonPropertyName("تاريخ_التقديم")]
        public string SubmittedAt { get; set; } = string.Empty;

        [JsonPropertyName("تاريخ_آخر_تحديث")]
        public string LastUpdatedAt { get; set; } = string.Empty;
    }
}
