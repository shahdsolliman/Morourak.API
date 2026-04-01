using System.ComponentModel.DataAnnotations;

namespace Morourak.Domain.Enums.Driving
{
    public enum DrivingLicenseCategory
    {
        [Display(Name = "قيادة خاصة")]
        Private,

        [Display(Name = "مهنية درجة أولى")]
        ProfessionalFirstDegree,

        [Display(Name = "مهنية درجة ثانية")]
        ProfessionalSecondDegree,

        [Display(Name = "مهنية درجة ثالثة")]
        ProfessionalThirdDegree,

        [Display(Name = "دراجة نارية")]
        Motorcycle,

        [Display(Name = "قيادة معدات ثقيلة")]
        HeavyEquipment,

        [Display(Name = "قيادة جرار زراعي")]
        AgriculturalTractor
    }
}
