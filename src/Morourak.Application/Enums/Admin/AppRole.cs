using System.ComponentModel.DataAnnotations;

namespace Morourak.Application.Enums.Admin;

public enum AppRole
{
    [Display(Name = "مواطن")]
    Citizen = 1,

    [Display(Name = "مفتش")]
    Inspector = 2,

    [Display(Name = "ممتحن")]
    Examinator = 3,

    [Display(Name = "مسؤول")]
    Admin = 4,

    [Display(Name = "طبيب")]
    Doctor = 5
}

