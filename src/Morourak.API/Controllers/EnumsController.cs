using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Morourak.Application.Enums.Admin;
using Morourak.Domain.Enums.Appointments;
using Morourak.Domain.Enums.Common;
using Morourak.Domain.Enums.Driving;
using Morourak.Domain.Enums.Request;
using Morourak.Domain.Enums.Vehicles;

namespace Morourak.API.Controllers;

/// <summary>
/// Provides enum catalogs for frontend/mobile clients (Arabic-first).
/// </summary>
[AllowAnonymous]
[Route("api/v1/meta/enums")]
[Tags("Meta")]
public sealed class EnumsController : BaseApiController
{
    [HttpGet]
    public IActionResult GetKeys()
    {
        return Success(new[]
        {
            "replacement-type",
            "appointment-type",
            "delivery-method",
            "vehicle-type",
            "driving-license-category",
            "service-type",
            "app-role",
            "user-sort-field"
        });
    }

    [HttpGet("{key}")]
    public IActionResult GetEnum(string key)
    {
        var enumType = key.Trim().ToLowerInvariant() switch
        {
            "replacement-type" => typeof(ReplacementType),
            "appointment-type" => typeof(AppointmentType),
            "delivery-method" => typeof(DeliveryMethod),
            "vehicle-type" => typeof(VehicleType),
            "driving-license-category" => typeof(DrivingLicenseCategory),
            "service-type" => typeof(ServiceType),
            "app-role" => typeof(AppRole),
            "user-sort-field" => typeof(UserSortField),
            _ => null
        };

        if (enumType == null)
        {
            return NotFound(new
            {
                isSuccess = false,
                message = "Enum key غير مدعوم.",
                errorCode = "ENUM_NOT_FOUND"
            });
        }

        return Success(BuildEnumCatalog(enumType));
    }

    private static object BuildEnumCatalog(Type enumType)
    {
        var items = new List<object>();
        foreach (var name in Enum.GetNames(enumType))
        {
            var value = Convert.ToInt32(Enum.Parse(enumType, name));
            items.Add(new
            {
                value,
                key = name,
                display = GetDisplayName(enumType, name) ?? name
            });
        }

        return new
        {
            enumName = enumType.Name,
            acceptedInputs = "Arabic display name (preferred), enum key (English), or numeric value.",
            items
        };
    }

    private static string? GetDisplayName(Type enumType, string name)
    {
        var member = enumType.GetMember(name, BindingFlags.Public | BindingFlags.Static).FirstOrDefault();
        return member?.GetCustomAttribute<DisplayAttribute>()?.Name?.Trim();
    }
}

