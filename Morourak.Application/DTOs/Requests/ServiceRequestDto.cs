using System;
using System.Text.Json.Serialization;

namespace Morourak.Application.DTOs
{
    /// <summary>
    /// Data transfer object for service request information.
    /// </summary>
    public class ServiceRequestDto
    {
        [JsonPropertyName("رقم_الطلب")]
        public string RequestNumber { get; set; } = default!;

        [JsonPropertyName("الرقم_القومي")]
        public string CitizenNationalId { get; set; } = default!;

        [JsonPropertyName("نوع_الخدمة")]
        public string ServiceType { get; set; } = null!;

        [JsonPropertyName("الحالة")]
        public string Status { get; set; } = null!;

        [JsonPropertyName("تاريخ_التقديم")]
        public DateTime SubmittedAt { get; set; }

        [JsonPropertyName("تاريخ_آخر_تحديث")]
        public DateTime? LastUpdatedAt { get; set; }

        [JsonPropertyName("المعرف_المرجعي")]
        public int ReferenceId { get; set; }

        [JsonPropertyName("الرسوم")]
        public ServiceRequestFeesDto Fees { get; set; } = new();

        [JsonPropertyName("بيانات_التوصيل")]
        public ServiceRequestDeliveryDto Delivery { get; set; } = new();

        [JsonPropertyName("بيانات_الدفع")]
        public ServiceRequestPaymentDto Payment { get; set; } = new();
    }

    public class ServiceRequestFeesDto
    {
        [JsonPropertyName("الرسوم_الأساسية")]
        public decimal BaseFee { get; set; }

        [JsonPropertyName("رسوم_التوصيل")]
        public decimal DeliveryFee { get; set; }

        [JsonPropertyName("إجمالي_المبلغ")]
        public decimal TotalAmount { get; set; }
    }

    public class ServiceRequestDeliveryDto
    {
        [JsonPropertyName("طريقة_التوصيل")]
        public string? Method { get; set; }

        [JsonPropertyName("العنوان")]
        public string? Address { get; set; }
    }

    public class ServiceRequestPaymentDto
    {
        [JsonPropertyName("الحالة")]
        public string Status { get; set; } = default!;

        [JsonPropertyName("رقم_العملية")]
        public string? TransactionId { get; set; }

        [JsonPropertyName("المبلغ")]
        public decimal? Amount { get; set; }

        [JsonPropertyName("الوقت")]
        public DateTime? Timestamp { get; set; }
    }
}