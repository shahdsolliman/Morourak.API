public class DrivingLicenseSeedModel
{
    public string LicenseNumber { get; set; } = null!;
    public string CitizenNationalId { get; set; } = null!;
    public string Category { get; set; } = null!;
    public DateTime IssueDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string Status { get; set; } = null!;
}
