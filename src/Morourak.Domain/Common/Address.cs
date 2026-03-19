
namespace Morourak.Domain.Common
{
    public class Address
    {
        public string Governorate { get; private set; } = default!;
        public string City { get; private set; } = default!;
        public string Details { get; private set; } = default!;

        private Address() { }

        public Address(string governorate, string city, string details)
        {
            Governorate = governorate;
            City = city;
            Details = details;
        }
    }
}
