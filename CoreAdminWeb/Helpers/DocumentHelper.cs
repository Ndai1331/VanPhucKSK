using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using QRCoder;
using Spire.Doc;
using Spire.Doc.Documents;
using Spire.Doc.Fields;
using static CoreAdminWeb.Services.Exports.ExportKSKDataService;

namespace CoreAdminWeb.Helpers
{
    public static class DocumentHelper
    {
        public const string QRCodeReplaceCode = "<<MaQR>>";

        public static void FindAndReplaceString(ref Document document, string oldValue, string newValue)
        {
            foreach (Section section in document.Sections)
            {
                foreach (Paragraph paragraph in section.Body.Paragraphs)
                {
                    ReplaceInParagraph(paragraph, oldValue, newValue);
                }

                foreach (Paragraph paragraph in section.Paragraphs)
                {
                    ReplaceInParagraph(paragraph, oldValue, newValue);
                }

                foreach (var child in section.Tables)
                {
                    if (child is Table table)
                    {
                        foreach (TableRow row in table.Rows)
                        {
                            foreach (TableCell cell in row.Cells)
                            {
                                foreach (Paragraph paragraph in cell.Paragraphs)
                                {
                                    ReplaceInParagraph(paragraph, oldValue, newValue);
                                }
                            }
                        }
                    }
                }

                FindTextBoxesInBody(section.Body, oldValue, newValue);
            }
        }

        public static void FillChiDinhKetQuaTable(ref Document doc, IEnumerable<CanLamSangItem> data)
        {
            var items = data.ToList();

            var nameSelections = doc.FindAllString("<<TenChiDinh>>", false, true);
            if (nameSelections.Length == 0)
            {
                return;
            }

            var row = nameSelections[0].GetAsOneRange().OwnerParagraph.Owner as TableRow;
            if (row == null)
            {
                return;
            }

            var table = row.Owner as Table;

            if (table == null)
            {
                return;
            }

            int insertIndex = table.Rows.IndexOf(row);

            for (int i = 0; i < items.Count; i++)
            {
                TableRow targetRow;
                if (i == 0)
                {
                    targetRow = row;
                }
                else
                {
                    targetRow = row.Clone();
                    table.Rows.Insert(insertIndex + i, targetRow);
                }

                foreach (TableCell cell in targetRow.Cells)
                {
                    foreach (Paragraph p in cell.Paragraphs)
                    {
                        foreach (DocumentObject obj in p.ChildObjects)
                        {
                            if (obj is TextRange tr)
                            {
                                if (tr.Text.Contains("<<TenChiDinh>>"))
                                {
                                    tr.Text = tr.Text.Replace("<<TenChiDinh>>", items[i].TenChiDinh);
                                }

                                if (tr.Text.Contains("<<kq_canlamsang>>"))
                                {
                                    tr.Text = tr.Text.Replace("<<kq_canlamsang>>", items[i].KetQua);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void FindTextBoxesInBody(Body body, string oldValue, string newValue)
        {
            foreach (DocumentObject obj in body.ChildObjects)
            {
                if (obj is Paragraph paragraph)
                {
                    foreach (DocumentObject child in paragraph.ChildObjects)
                    {
                        if (child is TextBox textBox)
                        {
                            foreach (Paragraph paragraphChild in textBox.Body.Paragraphs)
                            {
                                ReplaceInParagraph(paragraphChild, oldValue, newValue);
                            }
                        }

                        if (child is ShapeObject shape)
                        {
                            ReplaceInShape(shape, oldValue, newValue);
                        }

                        if (child is Table table)
                        {
                            foreach (TableRow row in table.Rows)
                            {
                                foreach (TableCell cell in row.Cells)
                                {
                                    FindTextBoxesInBody(cell, oldValue, newValue);
                                }
                            }
                        }
                    }
                }
                else if (obj is Table table)
                {
                    foreach (TableRow row in table.Rows)
                    {
                        foreach (TableCell cell in row.Cells)
                        {
                            FindTextBoxesInBody(cell, oldValue, newValue);
                        }
                    }
                }
            }
        }

        private static void ReplaceInShape(ShapeObject shape, string oldValue, string newValue)
        {
            foreach (Paragraph paragraph in shape.ChildObjects.OfType<Paragraph>())
            {
                ReplaceInParagraph(paragraph, oldValue, newValue);
            }
        }

        private static void ReplaceInParagraph(Paragraph paragraph, string oldValue, string newValue)
        {
            string fullText = paragraph.Text;
            int startIndex = fullText.IndexOf(oldValue);
            if (startIndex == -1)
            {
                return;
            }

            // Nếu không có xuống dòng, dùng Replace đơn giản để giữ format
            if (!newValue.Contains("\n"))
            {
                if (oldValue.Equals(QRCodeReplaceCode))
                {
                    paragraph.Replace(oldValue, "", false, true);

                    QRCodeGenerator qrGenerator = new QRCodeGenerator();
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(newValue, QRCodeGenerator.ECCLevel.Q);
                    PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                    byte[] qrCodeImage = qrCode.GetGraphic(20);

                    // Add the image
                    DocPicture picture = paragraph.AppendPicture(qrCodeImage);
                    // Optional: Adjust image size and position
                    picture.Width = 100;
                    picture.Height = 100;
                    picture.WrapType = TextWrappingStyle.InFrontOfText;
                }
                else
                {
                    paragraph.Replace(oldValue, newValue, false, true); // Giữ nguyên định dạng
                }

                return;
            }

            // Lưu trữ tất cả TextRange và vị trí của chúng
            var textRanges = new List<(TextRange Range, int Start, int Length)>();
            int currentPos = 0;
            foreach (var item in paragraph.Items)
            {
                if (item is TextRange tr)
                {
                    textRanges.Add((tr, currentPos, tr.Text.Length));
                    currentPos += tr.Text.Length;
                }
            }

            // Tìm các TextRange bị ảnh hưởng bởi oldValue
            var affectedRanges = textRanges
                .Where(tr => tr.Start < startIndex + oldValue.Length && tr.Start + tr.Length > startIndex)
                .ToList();

            if (!affectedRanges.Any())
            {
                return;
            }

            // Xóa nội dung cũ trong paragraph
            paragraph.Items.Clear();

            // Thêm lại phần trước oldValue
            if (startIndex > 0)
            {
                string beforeText = fullText.Substring(0, startIndex);
                foreach (var tr in textRanges)
                {
                    if (tr.Start < startIndex)
                    {
                        int length = Math.Min(tr.Length, startIndex - tr.Start);
                        TextRange newRange = paragraph.AppendText(beforeText.Substring(tr.Start, length));
                        CopyCharacterFormat(newRange, tr.Range);
                    }
                }
            }

            // Thêm newValue với xuống dòng
            string[] lines = newValue.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                if (!string.IsNullOrEmpty(lines[i]))
                {
                    // Áp dụng định dạng từ TextRange đầu tiên bị thay thế
                    TextRange newRange = paragraph.AppendText(lines[i]);
                    CopyCharacterFormat(newRange, affectedRanges[0].Range);
                }
                if (i < lines.Length - 1)
                {
                    paragraph.AppendBreak(BreakType.LineBreak);
                }
            }

            // Thêm lại phần sau oldValue
            int endIndex = startIndex + oldValue.Length;
            if (endIndex < fullText.Length)
            {
                string afterText = fullText.Substring(endIndex);
                foreach (var tr in textRanges)
                {
                    if (tr.Start + tr.Length > endIndex)
                    {
                        int offset = Math.Max(0, endIndex - tr.Start);
                        int length = tr.Length - offset;
                        if (length > 0)
                        {
                            TextRange newRange = paragraph.AppendText(afterText.Substring(tr.Start + offset - endIndex, length));
                            CopyCharacterFormat(newRange, tr.Range);
                        }
                    }
                }
            }
        }

        private static void CopyCharacterFormat(TextRange target, TextRange source)
        {
            target.CharacterFormat.FontName = source.CharacterFormat.FontName;
            target.CharacterFormat.FontSize = source.CharacterFormat.FontSize;
            target.CharacterFormat.Bold = source.CharacterFormat.Bold;
            target.CharacterFormat.Italic = source.CharacterFormat.Italic;
            target.CharacterFormat.UnderlineStyle = source.CharacterFormat.UnderlineStyle;
            target.CharacterFormat.TextColor = source.CharacterFormat.TextColor;
        }

        public static async Task MergePdfFiles(string[] inputFiles, string outputFile)
        {
            int batchSize = 300;
            int parallelBatches = 4;
            string tempDir = $"temp_merged_{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}";
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }

            var batches = inputFiles.Select((file, index) => new { File = file, Batch = index / batchSize })
                                  .GroupBy(x => x.Batch)
                                  .Select(g => g.Select(x => x.File).ToList())
                                  .ToList();

            // Merge batches in parallel to create intermediate PDFs
            var intermediatePdfs = new List<string>();
            var tasks = new List<Task>();

            for (int i = 0; i < batches.Count; i += parallelBatches)
            {
                var currentBatches = batches.Skip(i).Take(parallelBatches);
                foreach (var batch in currentBatches)
                {
                    string outputPath = Path.Combine(tempDir, $"intermediate_{Guid.NewGuid()}.pdf");
                    tasks.Add(Task.Run(() => MergeBatch(batch, outputPath)));
                    intermediatePdfs.Add(outputPath);
                }
                await Task.WhenAll(tasks);
                tasks.Clear();
                GC.WaitForPendingFinalizers();
                Console.WriteLine($"Processed batch group {i + 1}/{batches.Count}");
            }

            // Merge intermediate PDFs into final PDF
            MergeBatch(intermediatePdfs, outputFile);

            // Clean up temporary files
            Directory.Delete(tempDir, true);

            Console.WriteLine("PDF merging completed!");
        }

        static void MergeBatch(List<string> inputFiles, string outputPath)
        {
            using (var outputDoc = new PdfDocument())
            {
                foreach (var inputFile in inputFiles)
                {
                    using (var inputDoc = PdfReader.Open(inputFile, PdfDocumentOpenMode.Import))
                    {
                        for (int i = 0; i < inputDoc.PageCount; i++)
                        {
                            outputDoc.AddPage(inputDoc.Pages[i]);
                        }
                    }
                }
                outputDoc.Save(outputPath);
            }
        }
    }
}
