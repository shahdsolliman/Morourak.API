using System.Text.Json.Serialization;

namespace Morourak.Application.DTOs.Requests.Arabic
{
    /// <summary>
    /// Represents a service request with full details and tracking information in Arabic.
    /// </summary>
    public class طلب_خدمةDto
    {
        /// <summary>Unique request number for tracking.</summary>
        [JsonPropertyName("رقم الطلب")]
        public string رقم_الطلب { get; set; } = string.Empty;

        /// <summary>The national ID of the requester.</summary>
        [JsonPropertyName("الرقم القومي")]
        public string الرقم_القومي { get; set; } = string.Empty;

        /// <summary>Type of service requested (e.g., Renewal, New Issuance).</summary>
        [JsonPropertyName("نوع الخدمة")]
        public string نوع_الخدمة { get; set; } = string.Empty;

        /// <summary>Current status of the request (e.g., Pending, Approved, Rejected).</summary>
        [JsonPropertyName("الحالة")]
        public string الحالة { get; set; } = string.Empty;

        /// <summary>The date and time the request was submitted.</summary>
        [JsonPropertyName("تاريخ التقديم")]
        public DateTime تاريخ_التقديم { get; set; }

        /// <summary>The date and time of the last status update.</summary>
        [JsonPropertyName("تاريخ آخر تحديث")]
        public DateTime تاريخ_آخر_تحديث { get; set; }

        /// <summary>External reference ID if applicable.</summary>
        [JsonPropertyName("الرقم المرجعي")]
        public string? الرقم_المرجعي { get; set; }

        /// <summary>Fees information associated with the request.</summary>
        [JsonPropertyName("الرسوم")]
        public رسوم_طلب_الخدمةDto الرسوم { get; set; } = new();

        /// <summary>Delivery information if the service includes shipping.</summary>
        [JsonPropertyName("التوصيل")]
        public توصيل_طلب_الخدمةDto التوصيل { get; set; } = new();

        /// <summary>Payment details for the request.</summary>
        [JsonPropertyName("الدفع")]
        public دفع_طلب_الخدمةDto الدفع { get; set; } = new();
    }

    /// <summary>Fees details for a service request.</summary>
    public class رسوم_طلب_الخدمةDto
    {
        /// <summary>The base fee for the service.</summary>
        [JsonPropertyName("الرسوم الأساسية")]
        public decimal الرسوم_الأساسية { get; set; }

        /// <summary>The delivery fee if applicable.</summary>
        [JsonPropertyName("رسوم التوصيل")]
        public decimal رسوم_التوصيل { get; set; }

        /// <summary>The total amount to be paid.</summary>
        [JsonPropertyName("المبلغ الإجمالي")]
        public decimal المبلغ_الإجمالي { get; set; }
    }

    /// <summary>Delivery details for a service request.</summary>
    public class توصيل_طلب_الخدمةDto
    {
        /// <summary>Method of delivery (e.g., Home, Traffic Unit).</summary>
        [JsonPropertyName("طريقة التوصيل")]
        public string طريقة_التوصيل { get; set; } = string.Empty;

        /// <summary>Detailed delivery address.</summary>
        [JsonPropertyName("العنوان")]
        public string العنوان { get; set; } = string.Empty;
    }

    /// <summary>Payment information for a service request.</summary>
    public class دفع_طلب_الخدمةDto
    {
        /// <summary>Payment status (e.g., Paid, Pending).</summary>
        [JsonPropertyName("الحالة")]
        public string الحالة { get; set; } = string.Empty;

        /// <summary>Payment transaction unique identifier.</summary>
        [JsonPropertyName("رقم العملية")]
        public string? رقم_العملية { get; set; }

        /// <summary>The total amount paid.</summary>
        [JsonPropertyName("المبلغ")]
        public decimal المبلغ { get; set; }

        /// <summary>Timestamp of the payment transaction.</summary>
        [JsonPropertyName("الوقت")]
        public DateTime? الوقت { get; set; }
    }
}
