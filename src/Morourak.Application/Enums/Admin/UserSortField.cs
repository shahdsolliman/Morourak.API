using System.ComponentModel.DataAnnotations;

namespace Morourak.Application.Enums.Admin;

public enum UserSortField
{
    [Display(Name = "تاريخ الإنشاء")]
    CreatedAt = 1,

    [Display(Name = "البريد الإلكتروني")]
    Email = 2,

    [Display(Name = "الاسم")]
    Name = 3
}

