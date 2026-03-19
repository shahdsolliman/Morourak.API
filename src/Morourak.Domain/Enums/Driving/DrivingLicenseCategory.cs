using System.ComponentModel.DataAnnotations;

namespace Morourak.Domain.Enums.Driving
{
    public enum DrivingLicenseCategory
    {
        [Display(Name = "خاصة")]
        Private,

        [Display(Name = "درجة أولى")]
        ProfessionalFirstDegree,

        [Display(Name = "درجة ثانية")]
        ProfessionalSecondDegree,

        [Display(Name = "درجة ثالثة")]
        ProfessionalThirdDegree,

        [Display(Name = "دراجة نارية")]
        Motorcycle,

        [Display(Name = "معدات ثقيلة")]
        HeavyEquipment,

        [Display(Name = "جرار زراعي")]
        AgriculturalTractor
    }
}
