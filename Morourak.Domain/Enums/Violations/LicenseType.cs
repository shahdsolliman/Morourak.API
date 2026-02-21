using System.ComponentModel.DataAnnotations;

namespace Morourak.Domain.Enums.Violations
{
    public enum LicenseType
    {
        [Display(Name = "رخصة قيادة")]
        Driving = 0,

        [Display(Name = "رخصة مركبة")]
        Vehicle = 1
    }
}
