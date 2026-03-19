using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Morourak.API.Extensions.JsonConverters;

/// <summary>
/// A custom JSON converter that serializes Enum values using their [Display(Name = "...")] attribute.
/// If the attribute is missing, it falls back to the Enum name as a string.
/// </summary>
public class ArabicEnumConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsEnum;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type converterType = typeof(EnumToDisplayNameConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }

    private class EnumToDisplayNameConverter<T> : JsonConverter<T> where T : struct, Enum
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // For reading, we support both string (Enum name) and int (Enum value)
            if (reader.TokenType == JsonTokenType.String)
            {
                string enumString = reader.GetString()!;
                if (Enum.TryParse<T>(enumString, true, out T result))
                {
                    return result;
                }
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                int enumInt = reader.GetInt32();
                return (T)Enum.ToObject(typeof(T), enumInt);
            }

            throw new JsonException($"Unable to convert \"{reader.GetString()}\" to Enum {typeof(T).Name}.");
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            var field = value.GetType().GetField(value.ToString());
            var displayAttribute = field?.GetCustomAttribute<DisplayAttribute>();

            if (displayAttribute != null && !string.IsNullOrEmpty(displayAttribute.Name))
            {
                writer.WriteStringValue(displayAttribute.Name);
            }
            else
            {
                writer.WriteStringValue(value.ToString());
            }
        }
    }
}
