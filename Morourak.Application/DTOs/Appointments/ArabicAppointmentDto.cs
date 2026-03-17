using System.Text.Json.Serialization;

namespace Morourak.Application.DTOs.Appointments
{
    /// <summary>
    /// DTO for appointment data with Arabic property names for response.
    /// </summary>
    public class ArabicAppointmentDto
    {
        [JsonPropertyName("رقم_الطلب")]
        public string RequestNumber { get; set; } = string.Empty;

        [JsonPropertyName("اسم_المتقدم")]
        public string ApplicantName { get; set; } = string.Empty;

        [JsonPropertyName("نوع_الاختبار")]
        public string TestType { get; set; } = string.Empty;

        [JsonPropertyName("الرقم_القومي")]
        public string NationalId { get; set; } = string.Empty;

        [JsonPropertyName("تاريخ_ووقت_الحجز")]
        public string ReservationDateTime { get; set; } = string.Empty;
    }
}
