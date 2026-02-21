namespace Morourak.Infrastructure.Persistence.SeedData
{
    /// <summary>
    /// Model for deserializing traffic-violations.json seed data.
    /// </summary>
    public class TrafficViolationSeedModel
    {
        public string ViolationNumber { get; set; } = null!;
        public string CitizenNationalId { get; set; } = null!;
        public string RelatedLicenseNumber { get; set; } = null!;
        public string LicenseType { get; set; } = null!;
        public string ViolationType { get; set; } = null!;
        public string LegalReference { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Location { get; set; } = null!;
        public DateTime ViolationDateTime { get; set; }
        public decimal FineAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public string Status { get; set; } = null!;
        public bool IsPayable { get; set; }
    }
}
