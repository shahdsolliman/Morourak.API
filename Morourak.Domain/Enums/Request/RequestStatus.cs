using System.ComponentModel.DataAnnotations;

namespace Morourak.Domain.Enums.Request
{
    public enum RequestStatus
    {
        [Display(Name = "قيد الانتظار")]
        Pending = 1,

        [Display(Name = "قيد التنفيذ")]
        InProgress = 2,

        [Display(Name = "مكتمل")]
        Completed = 3,

        [Display(Name = "ملغى")]
        Cancelled = 4,


        [Display(Name = "فشل")]
        Failed = 5,

        [Display(Name = "جاهز للتنفيذ")]
        ReadyForProcessing = 6,

        [Display(Name = "في انتظار الدفع")]
        AwaitingPayment = 7,

        [Display(Name = "تم الدفع")]
        Paid = 8
    }
}