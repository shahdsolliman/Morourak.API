using System.ComponentModel.DataAnnotations;

namespace Morourak.Domain.Enums.Vehicles
{
    public enum VehicleType
    {
        [Display(Name = "ملاكي")]       // API: "Private"
        PrivateCar = 0,

        [Display(Name = "نقل")]
        Truck = 1,

        [Display(Name = "أجرة")]
        Taxi = 2,

        [Display(Name = "دراجة نارية")]
        Motorcycle = 3,

        [Display(Name = "أتوبيس")]
        Bus = 4,

        [Display(Name = "أتوبيس خاص")]
        PrivateBus = 5,

        [Display(Name = "مقطورة")]
        Trailer = 6
    }
}