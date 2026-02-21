using Morourak.Application.DTOs.Delivery;
using Morourak.Domain.Common;
using Morourak.Domain.Entities;
using Morourak.Domain.Enums.Common;

namespace Morourak.Application.Services
{
    public static class DeliveryFactory
    {
        public static void ApplyDelivery(
            VehicleLicense license,
            DeliveryInfoDto dto)
        {
            license.DeliveryMethod = dto.Method;

            if (dto.Method == DeliveryMethod.HomeDelivery)
            {
                if (dto.Address == null)
                    throw new InvalidOperationException("Address required for home delivery.");

                license.DeliveryAddress = new Address(
                    dto.Address.Governorate,
                    dto.Address.City,
                    dto.Address.Details);
            }
            else
            {
                license.DeliveryAddress = null;
            }
        }


        public static void ApplyDelivery(
            DrivingLicense license,
            DeliveryInfoDto dto)
        {
            license.DeliveryMethod = dto.Method;

            if (dto.Method == DeliveryMethod.HomeDelivery)
            {
                if (dto.Address == null)
                    throw new InvalidOperationException("Address required for home delivery.");

                license.DeliveryAddress = new Address(
                    dto.Address.Governorate,
                    dto.Address.City,
                    dto.Address.Details);
            }
            else
            {
                license.DeliveryAddress = null;
            }
        }
    }
}