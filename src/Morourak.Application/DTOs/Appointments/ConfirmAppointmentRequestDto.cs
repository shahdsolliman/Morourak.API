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

        /// <summary>Scheduled date for the appointment.</summary>
        /// <example>2025-12-01</example>
        public DateOnly Date { get; set; }

        /// <summary>Scheduled start time for the appointment.</summary>
        /// <example>09:00</example>
        public TimeOnly Time { get; set; }

        /// <summary>The ID of the selected governorate.</summary>
        public int GovernorateId { get; set; }

        /// <summary>The ID of the selected traffic unit (must belong to the selected governorate).</summary>
        public int TrafficUnitId { get; set; }
    }
}
