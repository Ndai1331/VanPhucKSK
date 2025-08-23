using Xceed.Document.NET;
using Xceed.Words.NET;

namespace CoreAdminWeb.Helpers
{
    public static class DocxImageReplacer
    {
        /// <summary>
        /// Thay mọi xuất hiện của placeholder bằng ảnh (width/height theo px) trên toàn tài liệu:
        /// Body, Header, Footer, Tables (kể cả lồng nhau). Không dùng SkiaSharp.
        /// </summary>
        public static void ReplacePlaceholderWithImage(
            DocX doc,
            string placeholder,
            byte[] imageBytes,
            int widthPx,
            int heightPx)
        {
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            if (string.IsNullOrEmpty(placeholder))
            {
                throw new ArgumentNullException(nameof(placeholder));
            }

            if (imageBytes == null || imageBytes.Length == 0)
            {
                throw new ArgumentNullException(nameof(imageBytes));
            }

            // Tạo Image một lần từ byte[], sau đó tạo Picture mới cho mỗi lần chèn
            using var ms = new MemoryStream(imageBytes);
            var image = doc.AddImage(ms);

            void ReplaceInParagraphs(IEnumerable<Paragraph> paragraphs)
            {
                foreach (var p in paragraphs)
                {
                    // Thay tất cả các lần xuất hiện trong cùng một paragraph
                    while (p.Text.Contains(placeholder))
                    {
                        var parts = p.Text.Split(new[] { placeholder }, 2, StringSplitOptions.None);

                        // Xoá text hiện tại rồi lắp lại cấu trúc: [trước] + [picture] + [sau]
                        p.ReplaceText(new StringReplaceTextOptions() { SearchValue = p.Text, NewValue = "" });
                        p.Append(parts[0]);

                        var pic = image.CreatePicture(widthPx, heightPx); // tạo instance mới mỗi lần chèn
                        p.AppendPicture(pic);

                        if (parts.Length > 1)
                        {
                            p.Append(parts[1]);
                        }
                    }
                }
            }

            // 1) Body
            ReplaceInParagraphs(doc.Paragraphs);
            foreach (var tbl in doc.Tables)
            {
                ReplaceInTable(tbl, ReplaceInParagraphs);
            }
        }

        private static void ReplaceInTable(Table table, Action<IEnumerable<Paragraph>> replace)
        {
            foreach (var row in table.Rows)
            {
                foreach (var cell in row.Cells)
                {
                    replace(cell.Paragraphs);
                    foreach (var nested in cell.Tables) // bảng lồng nhau
                    {
                        ReplaceInTable(nested, replace);
                    }
                }
            }
        }
    }
}
