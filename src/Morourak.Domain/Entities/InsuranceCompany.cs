using Morourak.Domain.Common;

namespace Morourak.Domain.Entities
{
    public class InsuranceCompany : BaseEntity<int>
    {
        public string Name { get; set; } = null!;
        public string NameAr { get; set; } = null!;
        public decimal Fee { get; set; }
        public string? Description { get; set; }
        public string? DescriptionAr { get; set; }
        public string? LogoPath { get; set; }
    }
}
