using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;

namespace CoreAdminWeb.Helpers
{
    public static class DocxHelper
    {
        public static void ReplaceText(this WordprocessingDocument doc, Dictionary<string, string> map)
        {
            var mp = doc.MainDocumentPart ?? throw new InvalidOperationException("Invalid document: no MainDocumentPart");
            var scopes = EnumerateSearchScopes(doc);

            // khóa dài trước ngắn
            var keys = map.Keys.OrderByDescending(k => k.Length).ToArray();

            foreach (var root in scopes)
            {
                foreach (var p in root.Descendants<Paragraph>())
                {
                    foreach (var key in keys)
                    {
                        // lặp nhiều lần trên cùng paragraph
                        while (true)
                        {
                            if (!TryBuildLinearAndMap(p, out var runs, out var linear, out var segs))
                            {
                                break;
                            }

                            int idx = IndexOfOrdinal(linear, key);
                            if (idx < 0)
                            {
                                break;
                            }

                            int idxEnd = idx + key.Length;

                            var affected = segs.Where(s => !(s.End <= idx || s.Start >= idxEnd))
                                               .OrderBy(s => s.Start).ToList();
                            if (affected.Count == 0)
                            {
                                break;
                            }

                            var first = affected[0];
                            var last = affected[affected.Count - 1];

                            int firstHeadLen = Math.Max(0, idx - first.Start);
                            int lastTailLen = Math.Max(0, last.End - idxEnd);

                            string firstHead = SafeSub(first.Text, 0, firstHeadLen);
                            string lastTail = SafeSub(last.Text, last.Text.Length - lastTailLen, lastTailLen);

                            bool sameTextEl = ReferenceEquals(first.TextEl, last.TextEl);

                            // Clear toàn bộ text trong dải
                            foreach (var s in affected)
                            {
                                s.TextEl.Text = string.Empty;
                            }

                            // Ghi lại prefix vào Text đầu
                            if (firstHead.Length > 0)
                            {
                                first.TextEl.Text = firstHead;
                                first.TextEl.Space = SpaceProcessingModeValues.Preserve;
                            }

                            // Chuẩn bị replacement run (style run CUỐI)
                            var innerRPr = PickInnerStyleRPr(runs, affected, idx, idxEnd);
                            int lastRunIdx = affected[affected.Count - 1].RunIndex;
                            var lastRunRPr = runs[lastRunIdx].RunProperties?.CloneNode(true) as RunProperties;

                            var anchorRun = runs[first.RunIndex];
                            var replaceRun = new Run();
                            if (innerRPr != null)
                            {
                                replaceRun.RunProperties = innerRPr;
                            }
                            else if (lastRunRPr != null)
                            {
                                replaceRun.RunProperties = (RunProperties)lastRunRPr.CloneNode(true);
                            }

                            string repl = (map[key] ?? string.Empty).Replace('\u00A0', ' ');
                            AppendTextWithPreservedWhitespace(replaceRun, repl);

                            if (sameTextEl)
                            {
                                // Case placeholder + suffix nằm chung 1 Text/Run
                                // → chèn replacement sau anchorRun, rồi chèn thêm 1 suffixRun sau replacement
                                anchorRun.Parent!.InsertAfter(replaceRun, anchorRun);

                                if (lastTail.Length > 0)
                                {
                                    var suffixRun = new Run();
                                    if (innerRPr != null)
                                    {
                                        suffixRun.RunProperties = (RunProperties)innerRPr.CloneNode(true);
                                    }
                                    else if (lastRunRPr != null)
                                    {
                                        suffixRun.RunProperties = (RunProperties)lastRunRPr.CloneNode(true);
                                    }

                                    AppendTextWithPreservedWhitespace(suffixRun, lastTail);
                                    replaceRun.Parent!.InsertAfter(suffixRun, replaceRun);
                                }
                            }
                            else
                            {
                                // Case thường: suffix nằm ở run/text khác → mình ghi suffix vào Text cuối,
                                // chèn replacement ngay sau anchorRun là đúng thứ tự
                                if (lastTail.Length > 0)
                                {
                                    last.TextEl.Text = lastTail;
                                    last.TextEl.Space = SpaceProcessingModeValues.Preserve;
                                }

                                anchorRun.Parent!.InsertAfter(replaceRun, anchorRun);
                            }

                            // Loop để tìm occurrence tiếp theo
                        }
                    }
                }
            }

            mp.Document.Save();
        }

        public static void ReplaceImage(this WordprocessingDocument doc, string placeholder, byte[]? imageBytes,
                                        string? appendText = null, int widthEmu = 990000, int heightEmu = 792000)
        {
            if (string.IsNullOrEmpty(placeholder))
            {
                throw new ArgumentException("placeholder is null/empty", nameof(placeholder));
            }

            var mp = doc.MainDocumentPart ?? throw new InvalidOperationException("Invalid document: no MainDocumentPart");
            bool hasImage = imageBytes != null && imageBytes.Length > 0;
            bool hasAppendText = !string.IsNullOrEmpty(appendText);

            ImagePart? imgPart = null;
            string? relId = null;
            if (hasImage)
            {
                imgPart = mp.AddImagePart(ImagePartType.Png);
                using (var ms = new MemoryStream(imageBytes!))
                {
                    imgPart.FeedData(ms);
                }

                relId = mp.GetIdOfPart(imgPart);
            }

            var scopes = EnumerateSearchScopes(doc);

            foreach (var root in scopes)
            {
                foreach (var p in root.Descendants<Paragraph>())
                {
                    while (true)
                    {
                        if (!TryBuildLinearAndMap(p, out var runs, out var linear, out var segs))
                        {
                            break;
                        }

                        int idx = IndexOfOrdinal(linear, placeholder);
                        if (idx < 0)
                        {
                            break;
                        }

                        int idxEnd = idx + placeholder.Length;

                        var affected = segs.Where(s => !(s.End <= idx || s.Start >= idxEnd))
                                           .OrderBy(s => s.Start).ToList();
                        if (affected.Count == 0)
                        {
                            break;
                        }

                        var first = affected[0];
                        var last = affected[affected.Count - 1];

                        int firstHeadLen = Math.Max(0, idx - first.Start);
                        int lastTailLen = Math.Max(0, last.End - idxEnd);

                        string firstHead = SafeSub(first.Text, 0, firstHeadLen);
                        string lastTail = SafeSub(last.Text, last.Text.Length - lastTailLen, lastTailLen);

                        bool sameTextEl = ReferenceEquals(first.TextEl, last.TextEl);

                        // Clear texts in affected
                        foreach (var s in affected)
                        {
                            s.TextEl.Text = string.Empty;
                        }

                        // Put back prefix
                        if (firstHead.Length > 0)
                        {
                            first.TextEl.Text = firstHead;
                            first.TextEl.Space = SpaceProcessingModeValues.Preserve;
                        }

                        var anchorRun = runs[first.RunIndex];

                        // Image run theo style run CUỐI
                        var innerRPr = PickInnerStyleRPr(runs, affected, idx, idxEnd);
                        int lastRunIdx = affected[affected.Count - 1].RunIndex;
                        var lastRunRPr = runs[lastRunIdx].RunProperties?.CloneNode(true) as RunProperties;

                        RunProperties? baseRPr = null;
                        if (innerRPr != null)
                        {
                            baseRPr = innerRPr;
                        }
                        else if (lastRunRPr != null)
                        {
                            baseRPr = (RunProperties)lastRunRPr.CloneNode(true);
                        }

                        // CASE 1: không có appendText => giữ nguyên behavior cũ: chỉ chèn image + giữ lastTail như code gốc
                        if (!hasAppendText)
                        {
                            if (!hasImage || relId == null)
                            {
                                // Không ảnh, không appendText: coi như xóa placeholder (đã clear ở trên).
                                // Nếu muốn giữ suffix cũ thì set lại lastTail.
                                if (!sameTextEl && lastTail.Length > 0)
                                {
                                    last.TextEl.Text = lastTail;
                                    last.TextEl.Space = SpaceProcessingModeValues.Preserve;
                                }
                                break;
                            }

                            var imageRun = new Run();
                            if (baseRPr != null)
                            {
                                imageRun.RunProperties = (RunProperties)baseRPr.CloneNode(true);
                            }

                            imageRun.AppendChild(BuildInlineImage(relId, widthEmu, heightEmu));

                            if (sameTextEl)
                            {
                                anchorRun.Parent!.InsertAfter(imageRun, anchorRun);

                                if (lastTail.Length > 0)
                                {
                                    var suffixRun = new Run();
                                    if (baseRPr != null)
                                    {
                                        suffixRun.RunProperties = (RunProperties)baseRPr.CloneNode(true);
                                    }

                                    AppendTextWithPreservedWhitespace(suffixRun, lastTail);
                                    imageRun.Parent!.InsertAfter(suffixRun, imageRun);
                                }
                            }
                            else
                            {
                                if (lastTail.Length > 0)
                                {
                                    last.TextEl.Text = lastTail;
                                    last.TextEl.Space = SpaceProcessingModeValues.Preserve;
                                }

                                anchorRun.Parent!.InsertAfter(imageRun, anchorRun);
                            }
                        }
                        // CASE 2: có appendText & có imageBytes => xóa empty line phía trên + ảnh trên, text dưới
                        else if (hasImage && relId != null)
                        {
                            // Xóa các paragraph trống phía trên, giữ lại paragraph hiện tại
                            RemoveEmptyParagraphsAbove(p);

                            var para = anchorRun.Ancestors<Paragraph>().FirstOrDefault() ?? p;

                            // Lấy ParagraphProperties “chuẩn” để canh giữa
                            ParagraphProperties? basePPr = ResolveSignatureParagraphProperties(para);

                            // Xóa toàn bộ run cũ trong paragraph placeholder
                            para.RemoveAllChildren<Run>();

                            // Set lại ParagraphProperties (center theo paragraph chuẩn)
                            if (basePPr != null)
                                para.ParagraphProperties = (ParagraphProperties)basePPr.CloneNode(true);

                            // Run ảnh
                            var imageRun = new Run();
                            if (baseRPr != null)
                                imageRun.RunProperties = (RunProperties)baseRPr.CloneNode(true);
                            imageRun.AppendChild(BuildInlineImage(relId, widthEmu, heightEmu));

                            // xuống dòng sau ảnh
                            var brRun = new Run(new Break());

                            // Run text (bác sĩ kết luận)
                            var textRun = new Run();
                            if (baseRPr != null)
                                textRun.RunProperties = (RunProperties)baseRPr.CloneNode(true);

                            AppendTextWithPreservedWhitespace(textRun, appendText!);

                            // Build lại paragraph: [image][br][text]
                            para.Append(imageRun);
                            para.Append(brRun);
                            para.Append(textRun);
                        }
                        // CASE 3: không có imageBytes => chỉ thay thế placeholder bằng appendText
                        else // hasAppendText && !hasImage
                        {
                            if (sameTextEl)
                            {
                                // Ghép luôn head + appendText + tail vào cùng TextEl
                                var combined = firstHead + appendText + lastTail;
                                first.TextEl.Text = combined;
                                first.TextEl.Space = SpaceProcessingModeValues.Preserve;
                            }
                            else
                            {
                                // Suffix ở run sau → giữ nguyên tại chỗ
                                if (lastTail.Length > 0)
                                {
                                    last.TextEl.Text = lastTail;
                                    last.TextEl.Space = SpaceProcessingModeValues.Preserve;
                                }

                                var appendRun = new Run();
                                if (baseRPr != null)
                                {
                                    appendRun.RunProperties = (RunProperties)baseRPr.CloneNode(true);
                                }
                                AppendTextWithPreservedWhitespace(appendRun, appendText!);
                                anchorRun.Parent!.InsertAfter(appendRun, anchorRun);
                            }
                        }
                    }
                }
            }

            mp.Document.Save();
        }

        public static void ReplaceTableRowsWithKqxn(this WordprocessingDocument doc, List<dynamic> kqxn)
        {
            if (doc.MainDocumentPart == null)
            {
                return;
            }

            var body = doc.MainDocumentPart.Document.Body;

            if (body == null)
            {
                return;
            }

            var table = body.Descendants<Table>()
                .FirstOrDefault(t => t.InnerText.Contains("<<TenXetNghiem>>"));

            if (table == null)
            {
                return;
            }

            // Find the template row (the one with placeholders)
            var templateRow = table.Descendants<TableRow>()
                .FirstOrDefault(r => r.InnerText.Contains("<<TenXetNghiem>>"));

            if (templateRow == null)
            {
                return;
            }

            // Insert new rows for each kqxn item
            foreach (var item in kqxn)
            {
                var newRow = (TableRow)templateRow.CloneNode(true);
                foreach (var cell in newRow.Descendants<TableCell>())
                {
                    var text = cell.InnerText;
                    cell.RemoveAllChildren<Paragraph>();
                    var cellPara = new Paragraph();
                    var innerParaRuner = new Run();
                    var innerText = new Text(text.Replace("<<TenXetNghiem>>", item.TenXetNghiem)
                            .Replace("<<KetQua>>", item.KetQua)
                            .Replace("<<ThamChieu>>", item.ThamChieu));
                    innerParaRuner.AppendChild(innerText);
                    cellPara.AppendChild(innerParaRuner);

                    cell.AppendChild(cellPara);
                }
                table.InsertBefore(newRow, templateRow);
            }

            // Remove the template row
            templateRow.Remove();
        }

        // ===== Utilities =====

        // Xây "view" tuyến tính của Paragraph + map ngược về Text element
        private sealed class TextSeg
        {
            public int RunIndex { get; init; }
            public Text TextEl { get; init; } = default!;
            public string Text { get; init; } = "";
            public int Start { get; init; } // inclusive (trong linear)
            public int End { get; init; } // exclusive
        }

        private static bool TryBuildLinearAndMap(Paragraph p, out List<Run> runs, out string linear, out List<TextSeg> segs)
        {
            runs = p.Elements<Run>().ToList();
            segs = new List<TextSeg>();
            var sb = new StringBuilder();
            int cursor = 0;

            if (runs.Count == 0)
            {
                linear = "";
                return false;
            }

            foreach (var (r, ri) in runs.Select((r, i) => (r, i)))
            {
                foreach (var child in r.ChildElements)
                {
                    switch (child)
                    {
                        case Text t:
                            string s = (t.Text ?? string.Empty).Replace('\u00A0', ' ');
                            if (s.Length > 0)
                            {
                                int start = cursor;
                                int end = cursor + s.Length;
                                segs.Add(new TextSeg { RunIndex = ri, TextEl = t, Text = s, Start = start, End = end });
                                sb.Append(s);
                                cursor = end;
                            }
                            break;

                        case Break:
                            sb.Append('\n'); cursor += 1;
                            break;

                        case TabChar:
                            sb.Append('\t'); cursor += 1;
                            break;

                        default:
                            // các phần tử khác (drawing/fieldchar/…) không tham gia text view
                            break;
                    }
                }
            }

            linear = sb.ToString();
            return linear.Length > 0;
        }

        private static string SafeSub(string s, int start, int len)
        {
            if (start < 0)
            {
                start = 0;
            }

            if (len < 0)
            {
                len = 0;
            }

            if (start > s.Length)
            {
                return string.Empty;
            }

            if (start + len > s.Length)
            {
                len = s.Length - start;
            }

            return s.Substring(start, len);
        }

        private static int IndexOfOrdinal(string haystack, string needle)
            => haystack.IndexOf(needle, StringComparison.Ordinal);

        private static void AppendTextWithPreservedWhitespace(Run run, string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return;
            }

            var parts = content.Split('\n');
            for (int i = 0; i < parts.Length; i++)
            {
                run.AppendChild(new Text(parts[i]) { Space = SpaceProcessingModeValues.Preserve });
                if (i < parts.Length - 1)
                {
                    run.AppendChild(new Break());
                }
            }
        }

        private static Drawing BuildInlineImage(string relId, int widthEmu, int heightEmu)
        {
            return new Drawing(
                new DW.Inline(
                    new DW.Extent() { Cx = widthEmu, Cy = heightEmu },
                    new DW.EffectExtent() { LeftEdge = 0L, TopEdge = 0L, RightEdge = 0L, BottomEdge = 0L },
                    new DW.DocProperties() { Id = (UInt32Value)1U, Name = "Picture" },
                    new DW.NonVisualGraphicFrameDrawingProperties(new A.GraphicFrameLocks() { NoChangeAspect = true }),
                    new A.Graphic(
                        new A.GraphicData(
                            new PIC.Picture(
                                new PIC.NonVisualPictureProperties(
                                    new PIC.NonVisualDrawingProperties() { Id = (UInt32Value)0U, Name = "Inserted Image" },
                                    new PIC.NonVisualPictureDrawingProperties()
                                ),
                                new PIC.BlipFill(new A.Blip() { Embed = relId }, new A.Stretch(new A.FillRectangle())),
                                new PIC.ShapeProperties(
                                    new A.Transform2D(new A.Offset() { X = 0L, Y = 0L }, new A.Extents() { Cx = widthEmu, Cy = heightEmu }),
                                    new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle }
                                )
                            )
                        )
                        { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" }
                    )
                )
                {
                    DistanceFromTop = 0U,
                    DistanceFromBottom = 0U,
                    DistanceFromLeft = 0U,
                    DistanceFromRight = 0U
                }
            );
        }

        // Bao quát mọi “story” để replace
        private static IEnumerable<OpenXmlElement> EnumerateSearchScopes(WordprocessingDocument doc)
        {
            var main = doc.MainDocumentPart ?? throw new InvalidOperationException("Invalid document: no MainDocumentPart");

            if (main.Document?.Body != null)
            {
                yield return main.Document.Body;
            }

            foreach (var hp in main.HeaderParts.Where(hp => hp.Header != null))
            {
                yield return hp.Header;
            }

            foreach (var fp in main.FooterParts.Where(fp => fp.Footer != null))
            {
                yield return fp.Footer;
            }

            if (main.FootnotesPart?.Footnotes != null)
            {
                yield return main.FootnotesPart.Footnotes;
            }

            if (main.EndnotesPart?.Endnotes != null)
            {
                yield return main.EndnotesPart.Endnotes;
            }

            if (main.WordprocessingCommentsPart?.Comments != null)
            {
                yield return main.WordprocessingCommentsPart.Comments;
            }
        }

        /// <summary>
        /// Chọn RunProperties theo phần văn bản NẰM BÊN TRONG [idx, idxEnd)
        /// Prefer: run có overlap lớn nhất. Fallback: null.
        /// </summary>
        /// <param name="runs"></param>
        /// <param name="affected"></param>
        /// <param name="idx"></param>
        /// <param name="idxEnd"></param>
        /// <returns></returns>
        private static RunProperties? PickInnerStyleRPr(
            List<Run> runs,
            List<TextSeg> affected,
            int idx, int idxEnd)
        {
            int bestRunIdx = -1;
            int bestOverlap = -1;

            foreach (var seg in affected)
            {
                // Overlap giữa [seg.Start, seg.End) và [idx, idxEnd)
                int start = Math.Max(seg.Start, idx);
                int end = Math.Min(seg.End, idxEnd);
                int overlap = Math.Max(0, end - start);

                if (overlap > bestOverlap)
                {
                    bestOverlap = overlap;
                    bestRunIdx = seg.RunIndex;
                }
            }

            if (bestRunIdx >= 0)
            {
                var rpr = runs[bestRunIdx].RunProperties;
                if (rpr != null)
                {
                    return (RunProperties)rpr.CloneNode(true);
                }
            }
            return null;
        }

        /// <summary>
        /// Xóa các Paragraph trống phía trên paragraph hiện tại
        /// (Paragraph chỉ chứa whitespace / xuống dòng, không có chữ thực sự).
        /// </summary>
        private static void RemoveEmptyParagraphsAbove(Paragraph paragraph)
        {
            OpenXmlElement? prev = paragraph.PreviousSibling();

            while (prev is Paragraph prevP)
            {
                // Có text không whitespace => dừng
                bool hasRealText = prevP.Descendants<Text>()
                                        .Any(t => !string.IsNullOrWhiteSpace(t.Text));
                if (hasRealText)
                    break;

                // Không có text thật => coi như linebreak/empty line, xóa
                var toRemove = prevP;
                prev = prevP.PreviousSibling();
                toRemove.Remove();
            }
        }

        private static ParagraphProperties? ResolveSignatureParagraphProperties(Paragraph para)
        {
            // 1. Ưu tiên PPr hiện tại nếu đã có Justification
            var currentPPr = para.ParagraphProperties;
            if (currentPPr?.GetFirstChild<Justification>() != null)
                return (ParagraphProperties)currentPPr.CloneNode(true);

            // 2. Nếu chưa, tìm paragraph có chữ phía trên để mượn alignment
            OpenXmlElement? prev = para.PreviousSibling();
            while (prev is Paragraph prevP)
            {
                bool hasRealText = prevP.Descendants<Text>()
                                        .Any(t => !string.IsNullOrWhiteSpace(t.Text));
                if (hasRealText)
                {
                    if (prevP.ParagraphProperties != null)
                        return (ParagraphProperties)prevP.ParagraphProperties.CloneNode(true);

                    break;
                }

                prev = prevP.PreviousSibling();
            }

            // 3. Nếu vẫn không có, default center
            return new ParagraphProperties(
                new Justification { Val = JustificationValues.Center });
        }
    }
}
