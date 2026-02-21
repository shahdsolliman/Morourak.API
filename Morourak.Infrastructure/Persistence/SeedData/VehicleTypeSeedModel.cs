using System.Collections.Generic;

namespace Morourak.Infrastructure.Persistence.SeedData
{
    public class VehicleTypeSeedModel
    {
        public string VehicleType { get; set; } = null!;
        public string VehicleTypeAr { get; set; } = null!;
        public List<BrandSeedModel> Brands { get; set; } = new List<BrandSeedModel>();
    }

    public class BrandSeedModel
    {
        public string Name { get; set; } = null!;
        public string NameAr { get; set; } = null!;
        public List<ModelSeedModel> Models { get; set; } = new List<ModelSeedModel>();
    }

    public class ModelSeedModel
    {
        public string Name { get; set; } = null!;
        public string NameAr { get; set; } = null!;
    }
}
