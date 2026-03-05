namespace Morourak.Infrastructure.Persistence.SeedData
{
    /// <summary>
    /// Represents the structure of citizen data coming from JSON file.
    /// Used only for seeding the database.
    /// </summary>
    public class CitizenRegistrySeedModel
    {
        public string NationalId { get; set; } = null!;
        public string MobileNumber { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
    }
}
