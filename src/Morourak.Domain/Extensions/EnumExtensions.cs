using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Morourak.Domain.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            if (enumValue == null) return string.Empty;

            var field = enumValue.GetType().GetField(enumValue.ToString());
            if (field == null) return enumValue.ToString();

            var attr = field.GetCustomAttribute<DisplayAttribute>();
            return attr != null ? attr.Name ?? enumValue.ToString() : enumValue.ToString();
        }
    }
}