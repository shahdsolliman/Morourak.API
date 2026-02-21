using System.Collections.Generic;

namespace Morourak.Application.DTOs.Vehicles
{
    public class VehicleTypeDetailDto
    {
        public int Value { get; set; }
        public string Name { get; set; } = null!;
        public string NameAr { get; set; } = null!;
        public List<BrandDetailDto> Brands { get; set; } = new();
    }

    public class BrandDetailDto
    {
        public string Name { get; set; } = null!;
        public string NameAr { get; set; } = null!;
        public List<ModelDetailDto> Models { get; set; } = new();
    }

    public class ModelDetailDto
    {
        public string Name { get; set; } = null!;
        public string NameAr { get; set; } = null!;
    }
}
