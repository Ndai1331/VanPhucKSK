using System.Reflection;

namespace CoreAdminWeb.Helpers
{
    public static class ReflectionHelper
    {
        public static void SetDateFieldValue<TValue1, TValue2>(TValue1 value1, TValue2 value2, string fieldName, DateTime? value)
        {
            if (EqualityComparer<TValue1>.Default.Equals(value1, default(TValue1)) || EqualityComparer<TValue2>.Default.Equals(value2, default(TValue2)) || string.IsNullOrWhiteSpace(fieldName))
            {
                return;
            }

            SetDateFieldValue(value1, fieldName, value);
            SetDateFieldValue(value2, fieldName, value);
        }
        public static void SetDateFieldValue<TTarget>(TTarget target, string fieldName, DateTime? value)
        {
            if (EqualityComparer<TTarget>.Default.Equals(target, default(TTarget)) || string.IsNullOrWhiteSpace(fieldName))
            {
                return;
            }

            var type = target?.GetType(); // Use null-conditional operator to prevent null dereference  
#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields  
            var property = type?.GetProperty(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
#pragma warning restore S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields  
            if (property != null && (property.PropertyType == typeof(DateTime?) || property.PropertyType == typeof(DateTime)))
            {
                property.SetValue(target, value);
                return;
            }

#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields  
            var field = type?.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
#pragma warning restore S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields  
            if (field != null && (field.FieldType == typeof(DateTime?) || field.FieldType == typeof(DateTime)))
            {
                field.SetValue(target, value);
            }
        }
    }
}
