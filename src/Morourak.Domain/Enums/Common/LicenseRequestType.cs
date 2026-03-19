using System.ComponentModel.DataAnnotations;

namespace Morourak.Domain.Enums.Common
{
    public enum LicenseRequestType
    {
        [Display(Name = "رخصة جديدة")]
        Initial = 0,

        [Display(Name = "تجديد رخصة")]
        Renewal = 1,

        [Display(Name = "بدل فاقد / بدل تالف")]
        Replacement = 2
    }
}