using System.ComponentModel.DataAnnotations;

namespace Morourak.Domain.Enums.Common
{
    public enum DeliveryMethod
    {
        [Display(Name = "وحدة المرور")]
        TrafficUnit = 1,

        [Display(Name = "توصيل للمنزل")]
        HomeDelivery = 2
    }
}