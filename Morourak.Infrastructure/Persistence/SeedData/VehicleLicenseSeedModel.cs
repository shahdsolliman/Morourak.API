public class VehicleLicenseSeedModel
{
    public string VehicleLicenseNumber { get; set; } = null!;
    public string CitizenNationalId { get; set; } = null!;
    public string PlateNumber { get; set; } = null!;
    public string VehicleType { get; set; } = null!;
    public string Brand { get; set; } = null!;
    public string Model { get; set; } = null!;
    public DateTime IssueDate { get; set; }
    public DateTime ExpiryDate { get; set; }

    public string ChassisNumber { get; set; } = null!;
    public string EngineNumber { get; set; } = null!;
    public string DeliveryMethod { get; set; } = null!; // TrafficUnit أو HomeDelivery
}