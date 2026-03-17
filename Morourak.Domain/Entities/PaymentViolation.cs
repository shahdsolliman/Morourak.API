namespace Morourak.Domain.Entities;

public class PaymentViolation
{
    public int PaymentId { get; set; }
    public Payment Payment { get; set; } = null!;

    public int TrafficViolationId { get; set; }
    public TrafficViolation TrafficViolation { get; set; } = null!;
    
    public decimal AmountPaid { get; set; }
}
