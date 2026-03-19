using System.ComponentModel.DataAnnotations;

namespace Morourak.Domain.Enums.Common
{
    public enum DeliveryStatus
    {
        [Display(Name = "قيد الانتظار")]
        Pending = 1,

        [Display(Name = "قيد التحضير")]
        Preparing = 2,

        [Display(Name = "تم التوصيل")]
        Delivered = 3
    }
}