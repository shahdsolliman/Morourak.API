using System.Text.Json.Serialization;

namespace Morourak.Application.DTOs.Appointments
{
    /// <summary>
    /// DTO for appointment data with Arabic property names for response.
    /// </summary>
    public class ArabicAppointmentDto
    {
        [JsonPropertyName("رقم الطلب")]
        public string RequestNumber { get; set; } = string.Empty;

        [JsonPropertyName("اسم المتقدم")]
        public string ApplicantName { get; set; } = string.Empty;

        [JsonPropertyName("نوع الاختبار")]
        public string TestType { get; set; } = string.Empty;

        [JsonPropertyName("الرقم القومي")]
        public string NationalId { get; set; } = string.Empty;

        [JsonPropertyName("تاريخ ووقت الحجز")]
        public string ReservationDateTime { get; set; } = string.Empty;
    }
}
