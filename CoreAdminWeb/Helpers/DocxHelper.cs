using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;

namespace CoreAdminWeb.Helpers
{
    public static class DocxHelper
    {


        public static void ReplaceImage(this WordprocessingDocument doc, string placeholder, byte[] imageBytes,
                                    int widthEmu = 990000, int heightEmu = 792000)
        {
            var mainPart = doc.MainDocumentPart;
            if (mainPart == null)
            {
                throw new Exception("Invalid document: no MainDocumentPart");
            }

            // Add image part
            var imagePart = mainPart.AddImagePart(ImagePartType.Png);
            using (var stream = new MemoryStream(imageBytes))
            {
                imagePart.FeedData(stream);
            }
            string relId = mainPart.GetIdOfPart(imagePart);

            // Duyệt tất cả Paragraph (cả trong table cell)
            foreach (var para in mainPart.Document.Descendants<Paragraph>())
            {
                string paraText = string.Concat(para.Descendants<Text>().Select(t => t.Text));

                if (paraText.Contains(placeholder))
                {
                    // Xoá toàn bộ Run chứa placeholder
                    foreach (var run in para.Descendants<Run>().ToList())
                    {
                        run.Remove();
                    }

                    // Tạo Run chứa ảnh
                    var drawing = new Drawing(
                        new DW.Inline(
                            new DW.Extent() { Cx = widthEmu, Cy = heightEmu },
                            new DW.EffectExtent()
                            {
                                LeftEdge = 0L,
                                TopEdge = 0L,
                                RightEdge = 0L,
                                BottomEdge = 0L
                            },
                            new DW.DocProperties() { Id = (UInt32Value)1U, Name = "Picture" },
                            new DW.NonVisualGraphicFrameDrawingProperties(
                                new A.GraphicFrameLocks() { NoChangeAspect = true }),
                            new A.Graphic(
                                new A.GraphicData(
                                    new PIC.Picture(
                                        new PIC.NonVisualPictureProperties(
                                            new PIC.NonVisualDrawingProperties()
                                            {
                                                Id = (UInt32Value)0U,
                                                Name = "Inserted Image"
                                            },
                                            new PIC.NonVisualPictureDrawingProperties()),
                                        new PIC.BlipFill(
                                            new A.Blip() { Embed = relId },
                                            new A.Stretch(new A.FillRectangle())),
                                        new PIC.ShapeProperties(
                                            new A.Transform2D(
                                                new A.Offset() { X = 0L, Y = 0L },
                                                new A.Extents() { Cx = widthEmu, Cy = heightEmu }),
                                            new A.PresetGeometry(new A.AdjustValueList())
                                            { Preset = A.ShapeTypeValues.Rectangle }))
                                )
                                { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
                        )
                        {
                            DistanceFromTop = 0U,
                            DistanceFromBottom = 0U,
                            DistanceFromLeft = 0U,
                            DistanceFromRight = 0U
                        });

                    para.AppendChild(new Run(drawing));
                }
            }

            mainPart.Document.Save();
        }
        public static void ReplaceText(this WordprocessingDocument doc, Dictionary<string, string> replacements)
        {
            var mainPart = doc.MainDocumentPart;
            if (mainPart == null)
            {
                return;
            }

            foreach (var para in mainPart.Document.Descendants<Paragraph>())
            {
                var runs = para.Elements<Run>().ToList();
                if (runs.Count == 0)
                {
                    continue;
                }

                
                foreach (var run in runs)
                {
                    var texts = run.Elements<Text>().ToList();
                    foreach (var text in texts)
                    {
                        var originalText = text.Text ?? "";
                        var replacedText = originalText;
                        
                        // Replace từng placeholder trong text này
                        foreach (var kv in replacements)
                        {
                            if (replacedText.Contains(kv.Key))
                            {
                                // Giữ nguyên khoảng trắng xung quanh placeholder
                                replacedText = replacedText.Replace(kv.Key, kv.Value ?? "");
                            }
                        }
                        
                        // Chỉ update text nếu có thay đổi
                        if (originalText != replacedText)
                        {
                            text.Text = replacedText;
                        }
                    }
                }
            }
            mainPart.Document.Save();
        }
        public static void ReplaceTextV2(this WordprocessingDocument doc, Dictionary<string, string> replacements)
        {
            var mainPart = doc.MainDocumentPart;
            if (mainPart == null) return;

            // Duyệt tất cả Text elements trong Body (bao gồm trong bảng và ngoài bảng)
            foreach (var text in mainPart.Document.Body.Descendants<Text>())
            {
                if (text.Text == null) continue;

                string updatedText = text.Text;

                // Replace từng placeholder nếu có
                foreach (var kv in replacements)
                {
                    if (updatedText.Contains(kv.Key))
                    {
                        updatedText = updatedText.Replace(kv.Key, kv.Value ?? "");
                    }
                }

                // Chỉ cập nhật text nếu có thay đổi
                if (updatedText != text.Text)
                {
                    text.Text = updatedText;
                }
            }

            mainPart.Document.Save();
        }

        public static void ReplaceParagraph(this WordprocessingDocument doc, string placeholder, Paragraph newParagraph)
        {
            var mainPart = doc.MainDocumentPart;
            if (mainPart == null) return;

            // Duyệt toàn bộ Text trong document (kể cả trong bảng)
            var texts = mainPart.Document.Body.Descendants<Text>().ToList();

            foreach (var text in texts)
            {
                if (text.Text != null && text.Text.Contains(placeholder))
                {
                    var runWithPlaceholder = text.Parent as Run;
                    var oldPara = text.Ancestors<Paragraph>().FirstOrDefault();
                    if (oldPara == null) continue;

                    // Clone pPr từ paragraph cũ
                    var oldPPr = oldPara.ParagraphProperties?.CloneNode(true) as ParagraphProperties;

                    // Clone rPr từ run chứa placeholder (ưu tiên dùng style này)
                    var srcRPr = runWithPlaceholder?.RunProperties?.CloneNode(true) as RunProperties;

                    // Tạo bản clone của paragraph mới để chỉnh style
                    var insertPara = (Paragraph)newParagraph.CloneNode(true);

                    // Áp pPr cũ nếu paragraph mới chưa có
                    if (oldPPr != null)
                    {
                        if (insertPara.ParagraphProperties == null)
                            insertPara.ParagraphProperties = (ParagraphProperties)oldPPr.CloneNode(true);
                        // nếu newParagraph đã có pPr thì giữ nguyên (không ghi đè)
                    }

                    // Áp rPr nguồn cho các run thiếu RunProperties
                    if (srcRPr != null)
                    {
                        foreach (var r in insertPara.Descendants<Run>())
                        {
                            if (r.RunProperties == null)
                                r.RunProperties = (RunProperties)srcRPr.CloneNode(true);

                            // Đảm bảo preserve space nếu có text thủ công
                            var t = r.GetFirstChild<Text>();
                            if (t != null) t.Space = SpaceProcessingModeValues.Preserve;
                        }
                    }
                    else
                    {
                        // fallback: nếu không có run nguồn, vẫn preserve space cho text
                        foreach (var r in insertPara.Descendants<Run>())
                        {
                            var t = r.GetFirstChild<Text>();
                            if (t != null) t.Space = SpaceProcessingModeValues.Preserve;
                        }
                    }

                    // Chèn và xoá theo đúng logic cũ
                    oldPara.Parent.InsertAfter(insertPara, oldPara);
                    oldPara.Remove();
                }
            }

            mainPart.Document.Save();
        }

        public static void ReplaceSmart(this WordprocessingDocument doc, Dictionary<string, string> replacements)
        {
            var mainPart = doc.MainDocumentPart;
            if (mainPart == null) return;

            // --- Ưu tiên replace theo Paragraph ---
            foreach (var kv in replacements)
            {
                var placeholder = kv.Key;
                var newText = kv.Value ?? "";

                var texts = mainPart.Document.Body.Descendants<Text>()
                                .Where(t => t.Text != null && t.Text.Contains(placeholder))
                                .ToList();

                foreach (var text in texts)
                {
                    var runWithPlaceholder = text.Parent as Run;
                    var oldPara = text.Ancestors<Paragraph>().FirstOrDefault();
                    if (oldPara == null) continue;

                    // Clone style của paragraph cũ
                    var oldPPr = oldPara.ParagraphProperties?.CloneNode(true) as ParagraphProperties;
                    var srcRPr = runWithPlaceholder?.RunProperties?.CloneNode(true) as RunProperties;

                    // Tạo paragraph mới
                    var newPara = new Paragraph();
                    if (oldPPr != null)
                        newPara.ParagraphProperties = (ParagraphProperties)oldPPr.CloneNode(true);

                    // Xử lý xuống dòng nếu có \n
                    var lines = newText.Split('\n');
                    foreach (var line in lines)
                    {
                        var run = new Run();
                        if (srcRPr != null)
                            run.RunProperties = (RunProperties)srcRPr.CloneNode(true);

                        run.Append(new Text(line) { Space = SpaceProcessingModeValues.Preserve });
                        newPara.Append(run);
                        newPara.Append(new Run(new Break()));
                    }

                    // Chèn paragraph mới thay paragraph cũ
                    oldPara.Parent.InsertAfter(newPara, oldPara);
                    oldPara.Remove();
                }
            }

            // --- Nếu chưa replace được thì fallback về ReplaceTextV2 ---
            var cells = mainPart.Document.Body.Descendants<TableCell>().ToList();

            foreach (var cell in cells)
            {
                var texts = cell.Descendants<Text>().ToList();
                if (texts.Count == 0) continue;

                string fullText = string.Join("", texts.Select(t => t.Text));
                bool hasReplace = false;

                foreach (var kv in replacements)
                {
                    if (fullText.Contains(kv.Key))
                    {
                        fullText = fullText.Replace(kv.Key, kv.Value ?? "");
                        hasReplace = true;
                    }
                }

                if (hasReplace)
                {
                    var firstPara = cell.Descendants<Paragraph>().FirstOrDefault();
                    var paraProps = firstPara?.ParagraphProperties?.CloneNode(true) as ParagraphProperties;
                    var firstRun = cell.Descendants<Run>().FirstOrDefault();
                    var runProps = firstRun?.RunProperties?.CloneNode(true) as RunProperties;

                    foreach (var t in texts) t.Remove();

                    var para = new Paragraph();
                    if (paraProps != null)
                        para.ParagraphProperties = (ParagraphProperties)paraProps.CloneNode(true);

                    var lines = fullText.Split('\n');
                    foreach (var line in lines)
                    {
                        var run = new Run();
                        if (runProps != null)
                            run.RunProperties = (RunProperties)runProps.CloneNode(true);

                        run.Append(new Text(line) { Space = SpaceProcessingModeValues.Preserve });
                        para.Append(run);
                        para.Append(new Run(new Break()));
                    }

                    cell.Append(para);
                }
            }

            mainPart.Document.Save();
        }

    }
}