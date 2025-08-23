using System.Reflection;

namespace CoreAdminWeb.Helpers
{
    public static class ReflectionHelper
    {
        public static void SetFieldValue<TValue1, TValue2>(TValue1 value1, TValue2 value2, string fieldName, object? value)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
            {
                return;
            }

            if (!EqualityComparer<TValue1>.Default.Equals(value1, default(TValue1)))
            {
                SetFieldValue(value1, fieldName, value);
            }

            if (!EqualityComparer<TValue2>.Default.Equals(value2, default(TValue2)))
            {
                SetFieldValue(value2, fieldName, value);
            }
        }

        public static void SetFieldValue<TTarget>(TTarget target, string fieldName, object? value)
        {
            if (EqualityComparer<TTarget>.Default.Equals(target, default(TTarget)) || string.IsNullOrWhiteSpace(fieldName))
            {
                return;
            }
            var type = target?.GetType();
#pragma warning disable S3011
            var property = type?.GetProperty(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
#pragma warning restore S3011
            if (property != null)
            {
                object? convertedValue = value;
                if (value != null && property.PropertyType != value.GetType())
                {
                    try
                    {
                        var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                        var invariantCulture = System.Globalization.CultureInfo.InvariantCulture;
                        if (targetType == typeof(decimal) || targetType == typeof(double) || targetType == typeof(float))
                        {
                            // Convert value to string and replace ',' with '.' if needed
                            string strValue = value.ToString() ?? string.Empty;
                            if (strValue.Contains(',') && !strValue.Contains('.'))
                            {
                                strValue = strValue.Replace(',', '.');
                            }
                            if (targetType == typeof(decimal))
                            {
                                convertedValue = Convert.ToDecimal(strValue, invariantCulture);
                            }
                            else if (targetType == typeof(double))
                            {
                                convertedValue = Convert.ToDouble(strValue, invariantCulture);
                            }
                            else if (targetType == typeof(float))
                            {
                                convertedValue = Convert.ToSingle(strValue, invariantCulture);
                            }
                        }
                        else
                        {
                            convertedValue = Convert.ChangeType(value, targetType, invariantCulture);
                        }
                    }
                    catch
                    {
                        // Ignore conversion errors
                    }
                }
                property.SetValue(target, convertedValue);
                return;
            }

#pragma warning disable S3011
            var field = type?.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
#pragma warning restore S3011
            if (field != null)
            {
                object? convertedValue = value;
                if (value != null && field.FieldType != value.GetType())
                {
                    try
                    {
                        var targetType = Nullable.GetUnderlyingType(field.FieldType) ?? field.FieldType;
                        var invariantCulture = System.Globalization.CultureInfo.InvariantCulture;
                        if (targetType == typeof(decimal) || targetType == typeof(double) || targetType == typeof(float))
                        {
                            string strValue = value.ToString() ?? string.Empty;
                            if (strValue.Contains(',') && !strValue.Contains('.'))
                            {
                                strValue = strValue.Replace(',', '.');
                            }
                            if (targetType == typeof(decimal))
                            {
                                convertedValue = Convert.ToDecimal(strValue, invariantCulture);
                            }
                            else if (targetType == typeof(double))
                            {
                                convertedValue = Convert.ToDouble(strValue, invariantCulture);
                            }
                            else if (targetType == typeof(float))
                            {
                                convertedValue = Convert.ToSingle(strValue, invariantCulture);
                            }
                        }
                        else
                        {
                            convertedValue = Convert.ChangeType(value, targetType, invariantCulture);
                        }
                    }
                    catch
                    {
                        // Ignore conversion errors
                    }
                }
                field.SetValue(target, convertedValue);
            }
        }
        public static object? GetFieldValue<TTarget>(TTarget target, string fieldName)
        {
            var type = target?.GetType(); // Use null-conditional operator to prevent null dereference  
#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields  
            var property = type?.GetProperty(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
#pragma warning restore S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields  
            if (property != null)
            {
                return property.GetValue(target);
            }

#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields  
            var field = type?.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
#pragma warning restore S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields  
            if (field != null)
            {
                return field.GetValue(target);
            }

            return null;
        }
    }
}
