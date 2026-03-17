using Morourak.Domain.Common;

namespace Morourak.Domain.Entities;

public class PaymentItem : BaseEntity<int>
{
    public int PaymentId { get; set; }
    public Payment Payment { get; set; } = null!;

    public string Description { get; set; } = null!;
    public decimal Amount { get; set; }
}
