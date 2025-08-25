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

                // Ghép toàn bộ text của paragraph
                var allText = string.Concat(runs.SelectMany(r => r.Elements<Text>()).Select(t => t.Text));

                // Replace tất cả placeholder trong allText
                var replacedText = allText;
                foreach (var kv in replacements)
                {
                    replacedText = replacedText.Replace(kv.Key, kv.Value ?? "");
                }

                // Nếu không có gì thay đổi thì bỏ qua
                if (allText == replacedText)
                {
                    continue;
                }

                // Phân bổ lại text vào các run cũ (giữ nguyên số lượng run và format)
                int pos = 0;
                foreach (var run in runs)
                {
                    foreach (var text in run.Elements<Text>())
                    {
                        int len = text.Text?.Length ?? 0;
                        if (pos + len > replacedText.Length)
                        {
                            text.Text = replacedText.Substring(pos);
                            pos = replacedText.Length;
                        }
                        else if (len > 0)
                        {
                            text.Text = replacedText.Substring(pos, len);
                            pos += len;
                        }
                    }
                }
                // Nếu còn dư text (do placeholder dài hơn tổng độ dài cũ), thêm vào run cuối
                if (pos < replacedText.Length)
                {
                    var lastRun = runs[runs.Count - 1];
                    var lastText = lastRun.Elements<Text>().LastOrDefault();
                    if (lastText != null)
                    {
                        lastText.Text += replacedText.Substring(pos);
                    }
                }
            }
            mainPart.Document.Save();
        }
    }
}