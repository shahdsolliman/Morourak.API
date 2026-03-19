using Morourak.Domain.Enums.Common;


namespace Morourak.Application.DTOs.Delivery
{
    public class DeliveryInfoDto
    {
        public DeliveryMethod Method { get; set; }
        public AddressDto? Address { get; set; }
    }
}
