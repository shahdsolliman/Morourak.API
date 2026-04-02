using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Morourak.API.Extensions.Swagger;

public sealed class ArabicEnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema == null) throw new ArgumentNullException(nameof(schema));
        if (context == null) throw new ArgumentNullException(nameof(context));

        var enumType = Nullable.GetUnderlyingType(context.Type) ?? context.Type;
        if (!enumType.IsEnum)
            return;

        var values = new List<IOpenApiAny>();
        foreach (var name in Enum.GetNames(enumType))
        {
            var displayName = GetDisplayName(enumType, name) ?? name;
            values.Add(new OpenApiString(displayName));
        }

        schema.Type = "string";
        schema.Format = null;
        schema.Enum = values;

        const string note =
            "Arabic display names are shown. The API also accepts enum keys (English) and numeric values (if defined).";

        schema.Description = string.IsNullOrWhiteSpace(schema.Description)
            ? note
            : $"{schema.Description} {note}";
    }

    private static string? GetDisplayName(Type enumType, string name)
    {
        var member = enumType.GetMember(name, BindingFlags.Public | BindingFlags.Static).FirstOrDefault();
        return member?.GetCustomAttribute<DisplayAttribute>()?.Name?.Trim();
    }
}

