public class DrivingLicenseSeedModel
{
    public string LicenseNumber { get; set; } = null!;
    public string CitizenNationalId { get; set; } = null!;
    public string Category { get; set; } = null!;
    public DateTime IssueDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string Status { get; set; } = null!;
    public string Governorate { get; set; }
    public string LicensingUnit { get; set; }
}
