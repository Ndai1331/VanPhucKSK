using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace CoreAdminWeb.Extensions
{
    public static class StringExtension
    {
        public static (string FirstName, string LastName) SplitName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return (string.Empty, string.Empty);
            }
            var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string lastName = "";
            string firstName = "";

            if (parts.Length > 0)
            {
                firstName = parts[^1]; // phần tử cuối là tên
                lastName = string.Join(" ", parts[..^1]); // ghép các phần còn lại thành họ
            }

            return (firstName, lastName);
        }

        public static string ToNormalChar(this string str, string rplStr = "")
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            string normalized = str.Normalize(NormalizationForm.FormD);
            StringBuilder result = new StringBuilder();

            foreach (char c in normalized)
            {
                UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(c);
                if (category != UnicodeCategory.NonSpacingMark)
                {
                    result.Append(c);
                }
            }

            string withoutDiacritics = result.ToString().Normalize(NormalizationForm.FormC);

            return Regex.Replace(withoutDiacritics, "[^a-zA-Z0-9_.]+", rplStr, RegexOptions.Compiled);
        }
    }
}
