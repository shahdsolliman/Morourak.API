public class VehicleLicenseSeedModel
{
    public string VehicleLicenseNumber { get; set; } = null!;
    public string CitizenNationalId { get; set; } = null!;
    public string PlateNumber { get; set; } = null!;
    public string VehicleType { get; set; } = null!;
    public string Brand { get; set; } = null!;
    public string Model { get; set; } = null!;
    public int ManufactureYear { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string Status { get; set; } = null!;

    public string Governorate { get; set; } = null!;
    public string ChassisNumber { get; set; } = null!;
    public string EngineNumber { get; set; } = null!;
    public string DeliveryMethod { get; set; } = null!; // TrafficUnit أو HomeDelivery
}