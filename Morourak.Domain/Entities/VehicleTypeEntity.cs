using Morourak.Domain.Common;

namespace Morourak.Domain.Entities
{
    public class VehicleTypeEntity : BaseEntity<int>
    {
        public string VehicleType { get; set; } = null!; // PrivateCar, Truck, etc.
        public string VehicleTypeAr { get; set; } = null!;
        public string Brand { get; set; } = null!;
        public string BrandAr { get; set; } = null!;
        public string Model { get; set; } = null!;
        public string ModelAr { get; set; } = null!;
    }
}