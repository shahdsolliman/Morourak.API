using Morourak.Domain.Enums.Common;

namespace Morourak.Domain.Common
{
    public interface IDeliveryInfo
    {
        DeliveryMethod DeliveryMethod { get; set; }
        Address? DeliveryAddress { get; set; }
    }
}
