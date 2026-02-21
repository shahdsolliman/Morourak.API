using System.ComponentModel.DataAnnotations;

namespace Morourak.Domain.Enums.Request
{
    public enum ServiceType
    {
        [Display(Name = "إصدار رخصة مركبة")]
        VehicleLicenseIssue = 1,

        [Display(Name = "تجديد رخصة مركبة")]
        VehicleLicenseRenewal = 2,

        [Display(Name = "استخراج بدل فاقد - رخصة مركبة")]
        VehicleLicenseReplacementLost = 3,

        [Display(Name = "استخراج بدل تالف - رخصة مركبة")]
        VehicleLicenseReplacementDamaged = 4,

        [Display(Name = "إصدار رخصة قيادة")]
        DrivingLicenseIssue = 5,

        [Display(Name = "استخراج بدل فاقد - رخصة قيادة")]
        DrivingLicenseReplacementLost = 6,


        [Display(Name = "استخراج بدل تالف - رخصة قيادة")]
        DrivingLicenseReplacementDamaged = 7,

        [Display(Name = "تجديد رخصة قيادة")]
        DrivingLicenseRenewal = 8,


        [Display(Name = "حجز فحص فني")]
        ExaminationTechnical = 9,

        [Display(Name = "حجز اختبار قيادة عملي")]
        ExaminationDriving = 10,

        [Display(Name = "تغيير فئة رخصة القيادة")]
        DrivingLicenseUpgrade = 11
    }
}