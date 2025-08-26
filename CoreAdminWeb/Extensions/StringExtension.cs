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

        /// <summary>
        /// Convert tiếng Việt có dấu thành không dấu.
        /// Tùy chọn: trim đầu/cuối, gom nhiều khoảng trắng, và thay khoảng trắng bằng ký tự/chuỗi bất kỳ (vd: "-", "_").
        /// </summary>
        /// <param name="input">Chuỗi nguồn</param>
        /// <param name="trimSpaces">Trim khoảng trắng đầu/cuối (mặc định: true)</param>
        /// <param name="spaceReplacement">Nếu khác null, thay tất cả khoảng trắng đơn bằng chuỗi này (vd: "-"). Mặc định: null = giữ khoảng trắng.</param>
        /// <param name="collapseSpaces">Gom nhiều khoảng trắng liên tiếp thành 1 khoảng trắng trước khi thay thế (mặc định: true)</param>
        public static string ToUnsign(this string input, bool trimSpaces = true, string? spaceReplacement = null, bool collapseSpaces = true)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input ?? string.Empty;
            }

            // Chuẩn hoá: mọi loại whitespace -> ' ' để xử lý dễ (tab, NBSP, line-break...)
            var sb = new StringBuilder(input.Length);
            foreach (var ch in input)
            {
                // Bỏ số 0-width thường gặp
                if (ch == '\u200B' || ch == '\uFEFF')
                {
                    continue;
                }

                sb.Append(char.IsWhiteSpace(ch) ? ' ' : ch);
            }

            // Normalize FormD để tách dấu, sau đó bỏ NonSpacingMark
            string formD = sb.ToString().Normalize(NormalizationForm.FormD);
            sb.Clear();

            foreach (var ch in formD)
            {
                // Riêng Đ/đ không phải dấu kết hợp → map thủ công
                if (ch == 'đ') { sb.Append('d'); continue; }
                if (ch == 'Đ') { sb.Append('D'); continue; }

                var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(ch);
                }
            }

            string res = sb.ToString().Normalize(NormalizationForm.FormC);

            // Gom nhiều space về 1 space nếu cần
            if (collapseSpaces)
            {
                sb.Clear();
                bool prevSpace = false;
                foreach (var ch in res)
                {
                    if (ch == ' ')
                    {
                        if (!prevSpace) { sb.Append(' '); prevSpace = true; }
                    }
                    else
                    {
                        sb.Append(ch);
                        prevSpace = false;
                    }
                }
                res = sb.ToString();
            }

            if (trimSpaces)
            {
                res = res.Trim();
            }

            // Thay khoảng trắng bằng ký tự/chuỗi tuỳ chọn
            if (spaceReplacement != null)
            {
                res = res.Replace(" ", spaceReplacement);
            }

            return res;
        }
    }
}
