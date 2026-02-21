using System.ComponentModel.DataAnnotations;

namespace Morourak.Domain.Enums.Violations
{
    public enum ViolationStatus
    {
        [Display(Name = "غير مدفوعة")]
        Unpaid = 0,

        [Display(Name = "مدفوعة جزئياً")]
        PartiallyPaid = 1,

        [Display(Name = "مدفوعة")]
        Paid = 2
    }
}
