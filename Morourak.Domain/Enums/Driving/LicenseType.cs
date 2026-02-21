
using System.ComponentModel.DataAnnotations;

namespace Morourak.Domain.Enums.Driving
{
    public enum LicenseType
    {
        [Display(Name = "خاصة")]
        Private = 1,

        [Display(Name = "مهنية")]
        Professional = 2,

        [Display(Name = "دراجة نارية")]
        Motorcycle = 3,

        [Display(Name = "خاصة لذوي الهمم")]
        Special = 4
    }
}
