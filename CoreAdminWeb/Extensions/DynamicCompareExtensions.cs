using System.Reflection;

namespace CoreAdminWeb.Extensions
{
    public static class DynamicCompareExtensions
    {
        /// <summary>
        /// So sánh giá trị property giữa value và dynamicObj, nếu khác thì trả về true.
        /// </summary>
        /// <param name="value">Giá trị hiện tại</param>
        /// <param name="dynamicObj">Đối tượng dynamic để so sánh</param>
        /// <param name="propertyName">Tên property cần so sánh</param>
        /// <returns>True nếu khác, false nếu giống hoặc không tìm thấy property</returns>
        public static bool IsValueChanged(this object? value, object? dynamicObj, string propertyName)
        {
            if (dynamicObj == null || string.IsNullOrEmpty(propertyName))
            {
                return false;
            }

            if (dynamicObj is IDictionary<string, object> expandoDict)
            {
                return expandoDict.ContainsKey(propertyName);
            }

            var dynamicType = dynamicObj.GetType();
            var prop = dynamicType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop == null)
            {
                return false;
            }

            var dynamicValue = prop.GetValue(dynamicObj);

            if (value == null && dynamicValue == null)
            {
                return false;
            }

            if (value == null || dynamicValue == null)
            {
                return true;
            }

            // Nếu value là object (không phải kiểu nguyên thủy hoặc string), so sánh theo trường "Id"
            var valueType = value.GetType();
            if (!valueType.IsPrimitive && valueType != typeof(string))
            {
                var idProp = valueType.GetProperty("id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                var dynamicIdProp = dynamicValue?.GetType().GetProperty("id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (idProp != null && dynamicIdProp != null)
                {
                    var valueId = idProp.GetValue(value);
                    var dynamicId = dynamicIdProp.GetValue(dynamicValue);
                    if (valueId == null && dynamicId == null)
                    {
                        return false;
                    }

                    if (valueId == null || dynamicId == null)
                    {
                        return true;
                    }

                    return !valueId.Equals(dynamicId);
                }
            }

            return !value.Equals(dynamicValue);
        }
        /// <summary>
        /// Trả về class CSS nếu giá trị bị thay đổi so với dynamicObj.
        /// </summary>
        /// <param name="value">Giá trị hiện tại</param>
        /// <param name="dynamicObj">Đối tượng dynamic để so sánh</param>
        /// <param name="propertyName">Tên property cần so sánh</param>
        /// <param name="highlightClass">Tên class highlight (mặc định: "border-red-500")</param>
        /// <returns>Chuỗi class CSS</returns>
        public static string GetHighlightClassIfChanged(this object? value, object? dynamicObj, string propertyName, string highlightClass = "border-red-500")
        {
            return value.IsValueChanged(dynamicObj, propertyName) ? highlightClass : string.Empty;
        }

        /// <summary>
        /// Trả về class CSS nếu tồn tại property trong dynamicObj.
        /// </summary>
        /// <param name="value">Giá trị hiện tại</param>
        /// <param name="dynamicObj">Đối tượng dynamic để so sánh</param>
        /// <param name="propertyName">Tên property cần kiểm tra</param>
        /// <param name="highlightClass">Tên class highlight (mặc định: "border-red-500")</param>
        /// <returns>Chuỗi class CSS</returns>
        public static string GetHighlightClassIfChanged(this object? dynamicObj, string propertyName, string highlightClass = "border-red-500")
        {
            if (dynamicObj == null || string.IsNullOrEmpty(propertyName))
            {
                return string.Empty;
            }

            // Handle ExpandoObject
            if (dynamicObj is IDictionary<string, object> expandoDict && expandoDict.ContainsKey(propertyName))
            {
                return highlightClass;
            }

            var dynamicType = dynamicObj.GetType();
            var prop = dynamicType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop != null)
            {
                return highlightClass;
            }

            var field = dynamicType.GetField(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (field != null)
            {
                return highlightClass;
            }

            return string.Empty;
        }

        public static object? GetIdOrValue(this object value)
        {
            if (value == null)
            {
                return null;
            }

            var type = value.GetType();
            if (type.IsClass && type != typeof(string))
            {
                var idProp = type.GetProperty("id");
                if (idProp != null)
                {
                    return idProp.GetValue(value);
                }
            }
            return value;
        }
    }
}
