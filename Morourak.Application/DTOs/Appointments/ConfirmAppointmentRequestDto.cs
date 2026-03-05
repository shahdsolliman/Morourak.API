namespace Morourak.Application.DTOs.Appointments
{
    /// <summary>
    /// Request body for POST /api/appointments/confirm
    /// Carries all data needed to create both a ServiceRequest and an Appointment
    /// in a single atomic operation.
    /// </summary>
    public class ConfirmAppointmentRequestDto
    {
        /// <summary>نوع الخدمة المطلوبة — مثال: "كشف طبي" | "فحص فني" | "اختبار قيادة"</summary>
        public string ServiceType { get; set; } = string.Empty;

        public DateOnly Date { get; set; }

        public TimeOnly Time { get; set; }

        /// <summary>المحافظة المختارة</summary>
        public int GovernorateId { get; set; }

        /// <summary>وحدة المرور المختارة — يجب أن تنتمي للمحافظة المختارة</summary>
        public int TrafficUnitId { get; set; }
    }
}
