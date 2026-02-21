using System.ComponentModel.DataAnnotations;

namespace Morourak.Domain.Enums.Violations
{
    public enum ViolationType
    {
        [Display(Name = "تجاوز السرعة القصوى")]
        SpeedLimitExceeded = 0,

        [Display(Name = "تجاوز الإشارة الحمراء")]
        RedLightViolation = 1,

        [Display(Name = "عدم ربط حزام الأمان")]
        SeatBeltViolation = 2,

        [Display(Name = "وقوف غير قانوني")]
        IllegalParking = 3,

        [Display(Name = "استخدام الهاتف أثناء القيادة")]
        MobilePhoneUsage = 4,

        [Display(Name = "القيادة بدون رخصة")]
        DrivingWithoutLicense = 5,

        [Display(Name = "القيادة برخصة منتهية")]
        ExpiredLicense = 6,

        [Display(Name = "تعديلات غير مصرح بها على المركبة")]
        UnauthorizedModification = 7
    }
}
