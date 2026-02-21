using System.ComponentModel.DataAnnotations;

namespace Morourak.Domain.Enums.Appointments
{
    public enum AppointmentType
    {
        //[Display(Name = "كشف طبي")]
        //Medical = 1,

        [Display(Name = "فحص فني")]
        Technical = 2,

        [Display(Name = "اختبار قيادة عملي")]
        Driving = 3,

        //[Display(Name = "اختبار الإشارات النظري")]
        //TrafficSignsTest = 4
    }
}