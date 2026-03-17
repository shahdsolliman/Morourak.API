using System.Text.Json.Serialization;

namespace Morourak.Application.DTOs.Requests
{
    /// <summary>
    /// كائن نقل بيانات لطلبات الخدمة بأسماء خصائص عربية واضحة.
    /// </summary>
    public class ServiceRequestArabicResponseDto
    {
        /// <summary>
        /// رقم الطلب الفريد في النظام.
        /// </summary>
        [JsonPropertyName("رقم_الطلب")]
        public string رقم_الطلب { get; set; } = string.Empty;

        /// <summary>
        /// الرقم القومي للمواطن مقدم الطلب.
        /// </summary>
        [JsonPropertyName("الرقم_القومي_للمواطن")]
        public string الرقم_القومي_للمواطن { get; set; } = string.Empty;

        /// <summary>
        /// نوع الخدمة المطلوبة (مثل: تجديد رخصة قيادة، إصدار رخصة مركبة).
        /// </summary>
        [JsonPropertyName("نوع_الخدمة")]
        public string نوع_الخدمة { get; set; } = string.Empty;

        /// <summary>
        /// حالة الطلب الحالية (مثل: قيد المراجعة، مقبول، مرفوض، مكتمل).
        /// </summary>
        [JsonPropertyName("حالة_الطلب")]
        public string حالة_الطلب { get; set; } = string.Empty;

        /// <summary>
        /// تاريخ تقديم الطلب بتنسيق عربي.
        /// </summary>
        [JsonPropertyName("تاريخ_التقديم")]
        public string تاريخ_التقديم { get; set; } = string.Empty;

        /// <summary>
        /// تاريخ آخر تحديث على حالة الطلب.
        /// </summary>
        [JsonPropertyName("تاريخ_آخر_تحديث")]
        public string? تاريخ_آخر_تحديث { get; set; }

        /// <summary>
        /// تفاصيل الرسوم المالية للطلب.
        /// </summary>
        [JsonPropertyName("تفاصيل_الرسوم")]
        public ServiceRequestFeesArabicDto الرسوم { get; set; } = new();

        /// <summary>
        /// تفاصيل التوصيل إن وجدت.
        /// </summary>
        [JsonPropertyName("تفاصيل_التوصيل")]
        public ServiceRequestDeliveryArabicDto التوصيل { get; set; } = new();

        /// <summary>
        /// تفاصيل الدفع الإلكتروني.
        /// </summary>
        [JsonPropertyName("تفاصيل_الدفع")]
        public ServiceRequestPaymentArabicDto الدفع { get; set; } = new();
    }

    /// <summary>
    /// تفاصيل الرسوم المالية باللغة العربية.
    /// </summary>
    public class ServiceRequestFeesArabicDto
    {
        /// <summary>
        /// الرسوم الأساسية للخدمة.
        /// </summary>
        [JsonPropertyName("الرسوم_الأساسية")]
        public decimal الرسوم_الأساسية { get; set; }

        /// <summary>
        /// رسوم التوصيل والخدمات الإضافية.
        /// </summary>
        [JsonPropertyName("رسوم_التوصيل")]
        public decimal رسوم_التوصيل { get; set; }

        /// <summary>
        /// إجمالي المبلغ المستحق.
        /// </summary>
        [JsonPropertyName("إجمالي_المبلغ")]
        public decimal إجمالي_المبلغ { get; set; }
    }

    /// <summary>
    /// تفاصيل التوصيل باللغة العربية.
    /// </summary>
    public class ServiceRequestDeliveryArabicDto
    {
        /// <summary>
        /// وسيلة التوصيل المختارة (مثل: عبر البريد، استلام شخصي).
        /// </summary>
        [JsonPropertyName("طريقة_التوصيل")]
        public string? طريقة_التوصيل { get; set; }

        /// <summary>
        /// عنوان التوصيل بالتفصيل.
        /// </summary>
        [JsonPropertyName("عنوان_التوصيل")]
        public string? عنوان_التوصيل { get; set; }
    }

    /// <summary>
    /// تفاصيل عملية الدفع باللغة العربية.
    /// </summary>
    public class ServiceRequestPaymentArabicDto
    {
        /// <summary>
        /// حالة الدفع (مثل: تم الدفع، في انتظار الدفع، فشلت العملية).
        /// </summary>
        [JsonPropertyName("حالة_الدفع")]
        public string حالة_الدفع { get; set; } = string.Empty;

        /// <summary>
        /// رقم العملية المرجعي من بوابة الدفع.
        /// </summary>
        [JsonPropertyName("رقم_العملية")]
        public string? رقم_العملية { get; set; }

        /// <summary>
        /// المبلغ الذي تم دفعه.
        /// </summary>
        [JsonPropertyName("المبلغ_المدفوع")]
        public decimal? المبلغ_المدفوع { get; set; }

        /// <summary>
        /// تاريخ ووقت عملية الدفع.
        /// </summary>
        [JsonPropertyName("تاريخ_الدفع")]
        public string? تاريخ_الدفع { get; set; }
    }
}
