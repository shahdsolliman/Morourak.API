using System.ComponentModel.DataAnnotations;

namespace Morourak.Domain.Enums
{
    public enum LicenseStatus
    {
        [Display(Name = "قيد الإنتظار")]
        Pending = 0,

        [Display(Name = "سارية")]
        Active = 1,

        [Display(Name = "منتهية")]
        Expired = 2,

        [Display(Name = "تمت الموافقة")]
        Approved = 3,

        [Display(Name = "مكتملة")]
        Completed = 4,

        [Display(Name = "مرفوضة")]
        Rejected = 5,

        [Display(Name = "تم الاستبدال")]
        Replaced = 6,
        DocumentsUploaded = 7,

        [Display(Name = " تجديد جاري")]
        PendingRenewal =8,
    }
}