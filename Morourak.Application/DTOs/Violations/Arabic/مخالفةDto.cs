using System.Text.Json.Serialization;
using Morourak.Domain.Enums.Violations;

namespace Morourak.Application.DTOs.Violations.Arabic
{
    /// <summary>
    /// Data transfer object representing a traffic violation in Arabic.
    /// </summary>
    public class مخالفةDto
    {
        /// <summary>Internal unique identifier for the violation.</summary>
        [JsonPropertyName("Id")]
        public int Id { get; set; }

        /// <summary>Official violation number or code.</summary>
        [JsonPropertyName("رقم المخالفة")]
        public string رقم_المخالفة { get; set; } = null!;

        /// <summary>Category or type of the violation (e.g., Speeding).</summary>
        [JsonPropertyName("نوع المخالفة")]
        public string نوع_المخالفة { get; set; } = null!;

        /// <summary>The legal article or law referenced by this violation.</summary>
        [JsonPropertyName("المادة القانونية")]
        public string المادة_القانونية { get; set; } = null!;

        /// <summary>Detailed description of the violation event.</summary>
        [JsonPropertyName("الوصف")]
        public string الوصف { get; set; } = null!;

        /// <summary>Physical location where the violation occurred.</summary>
        [JsonPropertyName("الموقع")]
        public string الموقع { get; set; } = null!;

        /// <summary>Date and time the violation was recorded.</summary>
        [JsonPropertyName("تاريخ ووقت المخالفة")]
        public string تاريخ_ووقت_المخالفة { get; set; } = null!;

        /// <summary>Total fine amount in EGP.</summary>
        [JsonPropertyName("قيمة الغرامة")]
        public decimal قيمة_الغرامة { get; set; }

        /// <summary>Amount already paid towards this fine.</summary>
        [JsonPropertyName("المبلغ المدفوع")]
        public decimal المبلغ_المدفوع { get; set; }

        /// <summary>Remaining amount to be paid.</summary>
        [JsonPropertyName("المبلغ المتبقي")]
        public decimal المبلغ_المتبقي { get; set; }

        /// <summary>Status of the violation (e.g., Unpaid, Paid).</summary>
        [JsonPropertyName("الحالة")]
        public string الحالة { get; set; } = null!;

        /// <summary>Indicates if the violation can be paid online.</summary>
        [JsonPropertyName("قابلة للدفع")]
        public bool قابلة_للدفع { get; set; }
    }
}
