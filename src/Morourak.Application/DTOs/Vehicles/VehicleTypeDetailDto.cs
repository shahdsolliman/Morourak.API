using System.Collections.Generic;

namespace Morourak.Application.DTOs.Vehicles
{
    /// <summary>
    /// Rich DTO providing hierarchical details of a vehicle type including brands and models.
    /// </summary>
    public class VehicleTypeDetailDto
    {
        /// <summary>Internal numeric value.</summary>
        public int Value { get; set; }

        /// <summary>English name of the vehicle type.</summary>
        public string Name { get; set; } = null!;

        /// <summary>Arabic name of the vehicle type.</summary>
        public string NameAr { get; set; } = null!;

        /// <summary>List of brands available for this vehicle type.</summary>
        public List<BrandDetailDto> Brands { get; set; } = new();
    }

    /// <summary>
    /// Details of a vehicle brand.
    /// </summary>
    public class BrandDetailDto
    {
        /// <summary>English brand name.</summary>
        public string Name { get; set; } = null!;

        /// <summary>Arabic brand name.</summary>
        public string NameAr { get; set; } = null!;

        /// <summary>List of models available for this brand.</summary>
        public List<ModelDetailDto> Models { get; set; } = new();
    }

    /// <summary>
    /// Details of a specific vehicle model.
    /// </summary>
    public class ModelDetailDto
    {
        /// <summary>English model name.</summary>
        public string Name { get; set; } = null!;

        /// <summary>Arabic model name.</summary>
        public string NameAr { get; set; } = null!;
    }
}
