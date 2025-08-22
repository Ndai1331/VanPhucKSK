using System.Text;
using System.Text.RegularExpressions;

namespace CoreAdminWeb.Extensions
{
    public static class TextWrapExtensions
    {
        /// <summary>
        /// Cắt chuỗi theo bề rộng tối đa (mm) KHÔNG dùng thư viện đồ họa.
        /// - Ước lượng bề rộng dựa trên hệ số (width factor) theo từng ký tự.
        /// - Tôn trọng xuống dòng \n: mỗi đoạn được wrap riêng.
        /// - Nếu một "từ" dài hơn khung, sẽ chẻ theo ký tự (breakLongWords=true).
        /// Lưu ý: Đây là phép đo ước lượng (heuristic), không chính xác tuyệt đối như render thật.
        /// </summary>
        public static IReadOnlyList<string> WrapToWidthMm(
            this string text,
            float maxWidthMm = 140f,
            string fontFamily = "Arial",   // giữ để tương thích chữ ký method
            float fontSizePt = 10f,
            float dpi = 96f,
            bool breakLongWords = true)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(text))
            {
                result.Add(string.Empty);
                return result;
            }

            // Quy đổi mm/pt -> px
            float pxPerMm = dpi / 25.4f;            // 1 inch = 25.4 mm
            float maxWidthPx = maxWidthMm * pxPerMm;
            float fontPx = fontSizePt * (dpi / 72f); // 1pt = 1/72 inch

            // Bảng hệ số bề rộng tương đối theo ký tự (x đơn vị em ~ fontPx)
            // Giá trị tham khảo: narrow ~0.35–0.45, normal ~0.5–0.6, wide ~0.7–0.95
            // (Heuristic cho font sans như Arial)
            static float CharFactor(char c)
            {
                if (c == '\t')
                {
                    return 2.0f;
                }

                if (char.IsWhiteSpace(c))
                {
                    return 0.33f;
                }

                // Rất hẹp
                const string narrow = ".,:;!|iI'`l[](){}";
                if (narrow.IndexOf(c) >= 0)
                {
                    return 0.38f;
                }

                // Rộng
                const string wide = "WM@#%&";
                if (wide.IndexOf(c) >= 0)
                {
                    return 0.92f;
                }

                // Số & ký tự thường
                if (char.IsDigit(c))
                {
                    return 0.55f;
                }

                // Dấu nối / toán tử
                const string med = "-_=+/*\\^~<>?";
                if (med.IndexOf(c) >= 0)
                {
                    return 0.5f;
                }

                // Chữ cái: ước lượng
                if (char.IsLetter(c))
                {
                    // Chữ hoa thường rộng hơn chút
                    return char.IsUpper(c) ? 0.60f : 0.53f;
                }

                // Ký tự khác (Unicode, tiếng Việt có dấu, v.v.)
                return 0.6f;
            }

            float MeasurePx(string s)
            {
                if (string.IsNullOrEmpty(s))
                {
                    return 0f;
                }

                float units = 0f;
                foreach (var ch in s)
                {
                    units += CharFactor(ch);
                }
                // 1em ≈ fontPx, nhân thêm hệ số “kerning/spacing” nhỏ (0.98) cho dễ khít
                return units * fontPx * 0.98f;
            }

            // Tôn trọng xuống dòng \n
            var paragraphs = Regex.Split(text, @"\r\n|\r|\n", RegexOptions.Compiled);
            for (int p = 0; p < paragraphs.Length; p++)
            {
                var para = paragraphs[p];

                if (para.Length == 0)
                {
                    result.Add(string.Empty);
                    continue;
                }

                // Tách token để giữ khoảng trắng
                var tokens = Regex.Split(para, @"(\s+)", RegexOptions.Compiled);

                var line = new StringBuilder();
                foreach (var token in tokens)
                {
                    if (token.Length == 0)
                    {
                        continue;
                    }

                    var candidate = line.ToString() + token;
                    if (MeasurePx(candidate) <= maxWidthPx || line.Length == 0)
                    {
                        line.Append(token);
                    }
                    else
                    {
                        // Đẩy dòng hiện tại
                        result.Add(line.ToString().TrimEnd());
                        line.Clear();

                        // Token tự nó quá rộng?
                        if (MeasurePx(token) > maxWidthPx && breakLongWords)
                        {
                            var chunk = new StringBuilder();
                            foreach (var ch in token)
                            {
                                var cand2 = chunk.ToString() + ch;
                                if (MeasurePx(cand2) > maxWidthPx && chunk.Length > 0)
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

            if (result.Count == 0)
            {
                result.Add(string.Empty);
            }

            return result;
        }
    }
}
