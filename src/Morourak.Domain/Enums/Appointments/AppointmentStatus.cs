using System.ComponentModel.DataAnnotations;

namespace Morourak.Domain.Enums.Appointments
{
    public enum AppointmentStatus
    {
        [Display(Name = "قيد الانتظار")]
        Pending = 1,

        [Display(Name = "محجوز")]
        Scheduled = 2,

        [Display(Name = "مكتمل")]
        Completed = 3,

        [Display(Name = "ملغى")]
        Cancelled = 4,

        [Display(Name = "ناجح")]
        Passed = 5,

        [Display(Name = "متاح")]
        Available = 6,

        [Display(Name = "راسب")]

        Failed
    }
}