using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Morourak.API.Extensions.EnumParsing;

internal static class EnumDisplayNameParser
{
    private sealed class EnumMap
    {
        public required Type EnumType { get; init; }
        public required Dictionary<string, object> ExactLookup { get; init; }
        public required (string? DisplayName, object Value)[] Members { get; init; }
        public required string AllowedValuesForError { get; init; }
    }

    private static readonly ConcurrentDictionary<Type, EnumMap> Cache = new();

    public static bool TryParse(Type enumType, string input, out object value)
    {
        if (enumType == null) throw new ArgumentNullException(nameof(enumType));

        var t = Nullable.GetUnderlyingType(enumType) ?? enumType;
        if (!t.IsEnum) throw new ArgumentException("Type must be an enum (or nullable enum).", nameof(enumType));

        input = (input ?? string.Empty).Trim();
        if (input.Length == 0)
        {
            value = default!;
            return false;
        }

        // Allow numeric values (but only if defined).
        if (int.TryParse(input, out var enumInt))
        {
            var boxed = Enum.ToObject(t, enumInt);
            if (Enum.IsDefined(t, boxed))
            {
                value = boxed;
                return true;
            }
        }

        // Allow enum names (case-insensitive) and also handles numeric strings, but we validate IsDefined.
        if (Enum.TryParse(t, input, ignoreCase: true, out var parsed) && parsed != null && Enum.IsDefined(t, parsed))
        {
            value = parsed;
            return true;
        }

        var map = GetOrCreateMap(t);

        // Exact match against enum names or [Display(Name)] values.
        if (map.ExactLookup.TryGetValue(input, out var exact))
        {
            value = exact;
            return true;
        }

        // Backward/forward-compat: allow strings that contain exactly one display name.
        // Example: "قيادة خاصة" should match Display(Name="خاصة").
        object? match = null;
        foreach (var (displayName, memberValue) in map.Members)
        {
            if (string.IsNullOrWhiteSpace(displayName)) continue;
            if (input.IndexOf(displayName, StringComparison.OrdinalIgnoreCase) < 0) continue;

            if (match != null)
            {
                value = default!;
                return false; // ambiguous contains-match
            }

            match = memberValue;
        }

        if (match != null)
        {
            value = match;
            return true;
        }

        value = default!;
        return false;
    }

    public static string GetAllowedValuesForError(Type enumType)
    {
        var t = Nullable.GetUnderlyingType(enumType) ?? enumType;
        var map = GetOrCreateMap(t);
        return map.AllowedValuesForError;
    }

    private static EnumMap GetOrCreateMap(Type enumType)
        => Cache.GetOrAdd(enumType, BuildMap);

    private static EnumMap BuildMap(Type enumType)
    {
        var exact = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        var members = new List<(string? DisplayName, object Value)>();

        foreach (var name in Enum.GetNames(enumType))
        {
            var value = Enum.Parse(enumType, name);
            exact[name] = value;

            var field = enumType.GetField(name, BindingFlags.Public | BindingFlags.Static);
            var display = field?.GetCustomAttribute<DisplayAttribute>()?.Name;
            if (!string.IsNullOrWhiteSpace(display))
                exact[display.Trim()] = value;

            members.Add((display?.Trim(), value));
        }

        var allowed = string.Join(", ",
            members.Select(m =>
            {
                var enumName = Enum.GetName(enumType, m.Value) ?? m.Value.ToString() ?? "";
                return string.IsNullOrWhiteSpace(m.DisplayName) ? enumName : $"{enumName} ({m.DisplayName})";
            }));

        return new EnumMap
        {
            EnumType = enumType,
            ExactLookup = exact,
            Members = members.ToArray(),
            AllowedValuesForError = allowed
        };
    }
}

