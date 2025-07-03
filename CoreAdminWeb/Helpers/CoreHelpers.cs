using System.Text.Json;

namespace CoreAdminWeb.Helpers
{
    public static class CoreHelpers
    {
        public const string DATE_FORMAT = "dd/MM/yyyy";
    /// <summary>
        /// Safe access to formatted date
        /// </summary>
        public static string FormatDate(DateTime? date, string format = DATE_FORMAT) => 
            date?.ToString(format) ?? string.Empty;

        /// <summary>
        /// Safe access to formatted date from string or DateTime
        /// </summary>
        public static string FormatDate(object? dateValue, string format = DATE_FORMAT)
        {
            if (dateValue == null) return string.Empty;

            if (dateValue is DateTime dateTime)
            {
                return dateTime.ToString(format);
            }

            if (dateValue is string dateString && !string.IsNullOrEmpty(dateString))
            {
                if (DateTime.TryParse(dateString, out var parsedDate))
                {
                    return parsedDate.ToString(format);
                }
                return dateString; // Return original string if can't parse
            }

            return string.Empty;
        }



        /// <summary>
        /// Safe access to boolean display
        /// </summary>
        public static string GetBooleanDisplay(bool? value, string trueText = "Có", string falseText = "Không") => 
            value.HasValue ? (value.Value ? trueText : falseText) : string.Empty;

        /// <summary>
        /// Safe access to string with default empty
        /// </summary>
        public static string GetSafeString(string? value) => value ?? string.Empty;

        /// <summary>
        /// Create simplified HTML content for large document fallback
        /// </summary>
    }
}