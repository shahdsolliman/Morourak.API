using System.ComponentModel.DataAnnotations;

namespace Morourak.Domain.Enums.Auth;

public enum UserStatus
{
    [Display(Name = "نشط")]
    Active = 1,

    [Display(Name = "معطّل")]
    Disabled = 2
}
