using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Morourak.Domain.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            var attr = enumValue.GetType()
                                .GetField(enumValue.ToString())
                                .GetCustomAttribute<DisplayAttribute>();

            return attr != null ? attr.Name : enumValue.ToString();
        }
    }
}