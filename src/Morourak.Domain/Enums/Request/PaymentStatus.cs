using System.ComponentModel.DataAnnotations;

namespace Morourak.Domain.Enums.Request;

public enum PaymentStatus
{
    [Display(Name = "قيد الانتظار")]
    Pending,

    [Display(Name = "تم الدفع")]
    Paid,

    [Display(Name = "فشل الدفع")]
    Failed
}
