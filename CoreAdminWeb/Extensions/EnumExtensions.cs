using System.ComponentModel;
using System.Reflection;

namespace CoreAdminWeb.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            FieldInfo? field = value.GetType().GetField(value.ToString());
            if (field == null) return value.ToString();
            DescriptionAttribute? attribute = field.GetCustomAttribute<DescriptionAttribute>();
            return attribute != null ? attribute.Description : value.ToString();
        }

        public static string GetDescriptionFromNumber<TEnum>(this int value) where TEnum : Enum
        {
            // Check if the value is defined in the enum
            if (!Enum.IsDefined(typeof(TEnum), value))
            {
                return value.ToString(); // Fallback to the integer value as a string if not found
            }

            // Get the enum member with the specified value
            var enumValue = (TEnum)Enum.ToObject(typeof(TEnum), value);
            var field = typeof(TEnum).GetField(enumValue.ToString());
            var attribute = field?.GetCustomAttributes(typeof(DescriptionAttribute), false)
                                .FirstOrDefault() as DescriptionAttribute;

            return attribute?.Description ?? enumValue.ToString();
        }

        public static string GetDescriptionFromString<TEnum>(this string value) where TEnum : Enum
        {
            if (int.TryParse(value, out int intValue))
            {
                return GetDescriptionFromNumber<TEnum>(intValue);
            }

            var enumType = typeof(TEnum);
            if (!enumType.IsEnum)
                throw new ArgumentException("Type must be an enum", nameof(enumType));

            if (Enum.TryParse(enumType, value, true, out var enumValue))
            {
                return ((Enum)enumValue).GetDescription();
            }

            return value; // Return input if parsing fails
        }

        public static string GetEnumDescription(this object enumValue)
        {
            var type = enumValue.GetType();
            var name = Enum.GetName(type, enumValue);
            if (name != null)
            {
                var field = type.GetField(name);
                if (field != null)
                {
                    var attr = field.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();
                    if (attr != null)
                        return attr.Description;
                }
            }
            return enumValue.ToString() ?? string.Empty;
        }

        public static string GetEnumDescription(this object? value, Type enumType)
        {
            if (value == null) return string.Empty;

            if (value is Enum)
            {
                var field = enumType.GetField(value.ToString() ?? string.Empty);
                var attr = field?.GetCustomAttribute<DescriptionAttribute>();
                return attr?.Description ?? value.ToString() ?? string.Empty;
            }

            if (value is string str)
            {
                if (Enum.TryParse(enumType, str, true, out var enumObj))
                {
                    var field = enumType.GetField(enumObj.ToString() ?? string.Empty);
                    var attr = field?.GetCustomAttribute<DescriptionAttribute>();
                    return attr?.Description ?? enumObj.ToString() ?? string.Empty;
                }
                return str;
            }

            return value.ToString() ?? string.Empty;
        }
    }
}
