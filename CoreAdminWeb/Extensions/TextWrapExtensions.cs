using SkiaSharp;
using System.Text;
using System.Text.RegularExpressions;

namespace CoreAdminWeb.Extensions
{
    public static class TextWrapExtensions
    {
        /// <summary>
        /// Cắt chuỗi theo bề rộng tối đa (mm), đo bằng font/size cho trước.
        /// - Tôn trọng xuống dòng \n: mỗi đoạn được wrap riêng.
        /// - Nếu một "từ" dài hơn khung, sẽ chẻ theo ký tự (breakLongWords=true).
        /// </summary>
        /// <param name="text">Chuỗi đầu vào</param>
        /// <param name="maxWidthMm">Bề rộng tối đa theo mm (mặc định 140mm)</param>
        /// <param name="fontFamily">Font (mặc định Arial)</param>
        /// <param name="fontSizePt">Cỡ chữ pt (mặc định 10pt)</param>
        /// <param name="dpi">DPI dùng để quy đổi pt & mm sang px (mặc định 96)</param>
        /// <param name="breakLongWords">Chẻ từ quá dài theo ký tự khi cần</param>
        public static IReadOnlyList<string> WrapToWidthMm(
            this string text,
            float maxWidthMm = 140f,
            string fontFamily = "Arial",
            float fontSizePt = 10f,
            float dpi = 96f,
            bool breakLongWords = true)
        {
            var result = new List<string>();

            if (text == null)
            {
                result.Add(string.Empty);
                return result;
            }

            // Quy đổi mm/pt -> px
            float pxPerMm = dpi / 25.4f;            // 1 inch = 25.4 mm
            float maxWidthPx = maxWidthMm * pxPerMm;
            float textSizePx = fontSizePt * (dpi / 72f); // 1pt = 1/72 inch

            using var typeface = SKTypeface.FromFamilyName(fontFamily) ?? SKTypeface.Default;
            using var paint = new SKPaint
            {
                Typeface = typeface,
                TextSize = textSizePx,
                IsAntialias = true,
                SubpixelText = true,
                LcdRenderText = true
            };

            float Measure(string s) => string.IsNullOrEmpty(s) ? 0 : paint.MeasureText(s);

            // Tôn trọng xuống dòng \n
            var paragraphs = Regex.Split(text, @"\r\n|\r|\n", RegexOptions.Compiled);

            for (int p = 0; p < paragraphs.Length; p++)
            {
                var para = paragraphs[p];

                // Đoạn rỗng -> giữ 1 dòng trống
                if (para.Length == 0)
                {
                    result.Add(string.Empty);
                    continue;
                }

                // Tách token để giữ lại khoảng trắng một cách hợp lý
                var tokens = Regex.Split(para, @"(\s+)", RegexOptions.Compiled);

                var line = new StringBuilder();
                foreach (var token in tokens)
                {
                    if (token.Length == 0)
                    {
                        continue;
                    }

                    var candidate = line.ToString() + token;
                    if (Measure(candidate) <= maxWidthPx || line.Length == 0)
                    {
                        line.Append(token);
                    }
                    else
                    {
                        // Đẩy dòng hiện tại
                        result.Add(line.ToString().TrimEnd());
                        line.Clear();

                        // Token tự nó đã quá rộng?
                        if (Measure(token) > maxWidthPx && breakLongWords)
                        {
                            // Chẻ theo ký tự
                            var chunk = new StringBuilder();
                            foreach (var ch in token)
                            {
                                var cand2 = chunk.ToString() + ch;
                                if (Measure(cand2) > maxWidthPx && chunk.Length > 0)
                                {
                                    result.Add(chunk.ToString());
                                    chunk.Clear();
                                    if (!char.IsWhiteSpace(ch))
                                    {
                                        chunk.Append(ch);
                                    }
                                }
                                else
                                {
                                    chunk.Append(ch);
                                }
                            }
                            line.Append(chunk.ToString());
                        }
                        else
                        {
                            // Bắt đầu dòng mới với token này; bỏ khoảng trắng đầu dòng
                            line.Append(token.TrimStart());
                        }
                    }
                }

                if (line.Length > 0)
                {
                    result.Add(line.ToString().TrimEnd());
                }
            }

            // Xử lý trường hợp toàn bộ input là chuỗi rỗng (không có \n)
            if (result.Count == 0)
            {
                result.Add(string.Empty);
            }

            return result;
        }
    }
}
