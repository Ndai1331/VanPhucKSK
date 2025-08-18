using System.ComponentModel;
using System.Reflection;

namespace CoreAdminWeb.Commons.Helpers
{
    public static class EnumHelper
    {
        public static List<(int Value, string Name, string Description, TEnum EnumValue)> ToList<TEnum>() where TEnum : Enum
        {
            var type = typeof(TEnum);
            var values = Enum.GetValues(type).Cast<TEnum>();
            var list = new List<(int, string, string, TEnum)>();

            foreach (var value in values)
            {
                var member = type.GetMember(value.ToString()).FirstOrDefault();
                var description = member?
                    .GetCustomAttribute<DescriptionAttribute>()?
                    .Description ?? value.ToString();

                list.Add(((int)(object)value, value.ToString(), description, value));
            }

            return list;
        }
    }
}
