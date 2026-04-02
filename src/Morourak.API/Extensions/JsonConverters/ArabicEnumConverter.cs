using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Morourak.API.Extensions.EnumParsing;

namespace Morourak.API.Extensions.JsonConverters;

/// <summary>
/// A custom JSON converter that serializes Enum values using their [Display(Name = "...")] attribute.
/// If the attribute is missing, it falls back to the Enum name as a string.
/// </summary>
public class ArabicEnumConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        var t = Nullable.GetUnderlyingType(typeToConvert) ?? typeToConvert;
        return t.IsEnum;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var enumType = Nullable.GetUnderlyingType(typeToConvert) ?? typeToConvert;

        if (enumType != typeToConvert)
        {
            var converterType = typeof(NullableEnumToDisplayNameConverter<>).MakeGenericType(enumType);
            return (JsonConverter)Activator.CreateInstance(converterType)!;
        }

        var nonNullableConverterType = typeof(EnumToDisplayNameConverter<>).MakeGenericType(enumType);
        return (JsonConverter)Activator.CreateInstance(nonNullableConverterType)!;
    }

    private class EnumToDisplayNameConverter<T> : JsonConverter<T> where T : struct, Enum
    {
        private static readonly IReadOnlyDictionary<T, string> DisplayNameByValue = BuildDisplayNameByValue();

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // For reading, we support both string (Enum name) and int (Enum value)
            if (reader.TokenType == JsonTokenType.String)
            {
                var enumString = reader.GetString() ?? string.Empty;
                if (EnumDisplayNameParser.TryParse(typeof(T), enumString, out var parsed))
                    return (T)parsed;
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                var enumInt = reader.GetInt32();
                var boxed = Enum.ToObject(typeof(T), enumInt);
                if (Enum.IsDefined(typeof(T), boxed))
                    return (T)boxed;
            }

            throw new JsonException($"قيمة غير صحيحة. القيم المسموحة: {EnumDisplayNameParser.GetAllowedValuesForError(typeof(T))}.");
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (DisplayNameByValue.TryGetValue(value, out var displayName))
                writer.WriteStringValue(displayName);
            else
                writer.WriteStringValue(value.ToString());
        }

        private static IReadOnlyDictionary<T, string> BuildDisplayNameByValue()
        {
            var result = new Dictionary<T, string>();
            foreach (var value in Enum.GetValues<T>())
            {
                var enumName = value.ToString();
                var field = typeof(T).GetField(enumName);
                var displayAttribute = field?.GetCustomAttribute<DisplayAttribute>();

                result[value] = !string.IsNullOrWhiteSpace(displayAttribute?.Name)
                    ? displayAttribute!.Name!.Trim()
                    : enumName;
            }

            return result;
        }
    }

    private class NullableEnumToDisplayNameConverter<T> : JsonConverter<T?> where T : struct, Enum
    {
        private static readonly EnumToDisplayNameConverter<T> Inner = new();

        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            return Inner.Read(ref reader, typeof(T), options);
        }

        public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            Inner.Write(writer, value.Value, options);
        }
    }
}
