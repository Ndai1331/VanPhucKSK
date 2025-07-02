using iText.Html2pdf;
using iText.Html2pdf.Resolver.Font;
using iText.IO.Font;
using iText.Kernel.Pdf;
using iText.Layout.Font;
using CoreAdminWeb.Model;
using System.Text;
using System.Text.RegularExpressions;

namespace CoreAdminWeb.Services.PDFService
{
    /// <summary>
    /// Manual PDF generation service using iText7 - No browser dependencies
    /// </summary>
    public class PdfService : IPdfService
    {
        private readonly ILogger<PdfService> _logger;

        public PdfService(ILogger<PdfService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Generate PDF from HTML content with default settings
        /// </summary>
        public byte[] GeneratePdfFromHtml(string htmlContent, string fileName = "document.pdf")
        {
            var defaultSettings = new PdfSettings
            {
                FileName = fileName,
                PageSize = "A4",
                Orientation = "Portrait",
                MarginTop = 10,
                MarginBottom = 10,
                MarginLeft = 10,
                MarginRight = 10
            };

            return GeneratePdfFromHtml(htmlContent, defaultSettings);
        }

        /// <summary>
        /// Generate PDF from HTML content with custom settings - Manual PDF generation
        /// </summary>
        public byte[] GeneratePdfFromHtml(string htmlContent, PdfSettings settings)
        {
            try
            {
                _logger.LogInformation("=== Starting Manual PDF generation with iText7 ===");
                _logger.LogInformation($"Input HTML size: {htmlContent.Length} characters");

                // Clean and optimize HTML content
                var cleanHtml = CleanHtmlForPdf(htmlContent);
                _logger.LogInformation($"Cleaned HTML size: {cleanHtml.Length} characters");

                // Create complete HTML document with CSS
                var completeHtml = CreateCompleteHtmlDocument(cleanHtml);
                _logger.LogInformation($"Complete HTML size: {completeHtml.Length} characters");

                // Generate PDF using iText7
                using var memoryStream = new MemoryStream();
                
                var converterProperties = new ConverterProperties();
                
                // Configure font provider for Vietnamese support
                var fontProvider = new DefaultFontProvider(true, true, true);
                
                // Add system fonts for better Vietnamese support
                try
                {
                    // Try to add common system fonts
                    AddSystemFonts(fontProvider);
                }
                catch (Exception fontEx)
                {
                    _logger.LogWarning(fontEx, "Could not add system fonts, using default fonts");
                }
                
                converterProperties.SetFontProvider(fontProvider);
                converterProperties.SetCharset("UTF-8");

                // Generate PDF
                _logger.LogInformation("Converting HTML to PDF...");
                HtmlConverter.ConvertToPdf(completeHtml, memoryStream, converterProperties);
                
                var pdfBytes = memoryStream.ToArray();
                _logger.LogInformation($"✅ PDF generated successfully! Size: {pdfBytes.Length} bytes ({pdfBytes.Length / 1024.0:F1} KB)");
                
                return pdfBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manual PDF generation failed");
                
                // Fallback: Create simple PDF if complex HTML fails
                _logger.LogInformation("Creating fallback PDF...");
                return CreateFallbackPdf(htmlContent, settings);
            }
        }

        /// <summary>
        /// Add system fonts for Vietnamese support
        /// </summary>
        private void AddSystemFonts(DefaultFontProvider fontProvider)
        {
            try
            {
                // macOS fonts
                var macFonts = new[]
                {
                    "/System/Library/Fonts/Times.ttc",
                    "/System/Library/Fonts/Arial.ttf",
                    "/System/Library/Fonts/Helvetica.ttc",
                    "/Library/Fonts/Times New Roman.ttf"
                };

                foreach (var fontPath in macFonts)
                {
                    if (File.Exists(fontPath))
                    {
                        fontProvider.AddFont(fontPath);
                        _logger.LogInformation($"Added font: {fontPath}");
                    }
                }

                // Windows fonts (if running on Windows)
                var windowsFonts = new[]
                {
                    @"C:\Windows\Fonts\times.ttf",
                    @"C:\Windows\Fonts\arial.ttf",
                    @"C:\Windows\Fonts\calibri.ttf"
                };

                foreach (var fontPath in windowsFonts)
                {
                    if (File.Exists(fontPath))
                    {
                        fontProvider.AddFont(fontPath);
                        _logger.LogInformation($"Added font: {fontPath}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error adding system fonts");
            }
        }

        /// <summary>
        /// Clean HTML content for PDF generation
        /// </summary>
        private string CleanHtmlForPdf(string htmlContent)
        {
            if (string.IsNullOrEmpty(htmlContent))
                return htmlContent;

            var cleaned = htmlContent;

            try
            {
                // Remove Blazor-specific attributes
                cleaned = Regex.Replace(cleaned, @"\s+b-[a-zA-Z0-9]+=""[^""]*""", "", RegexOptions.IgnoreCase);
                
                // Remove Blazor comment markers
                cleaned = cleaned.Replace("<!--!-->", "");
                
                // Remove problematic attributes
                cleaned = cleaned.Replace(" loading=\"lazy\"", "");
                cleaned = cleaned.Replace(" data-aos=", " data-removed-aos=");
                
                // Clean up whitespace
                cleaned = Regex.Replace(cleaned, @"\s+", " ");
                cleaned = Regex.Replace(cleaned, @">\s+<", "><");
                
                // Fix image sources for PDF
                cleaned = Regex.Replace(cleaned, @"src=""data:image/svg\+xml[^""]*""", "src=\"\"");
                
                _logger.LogInformation("HTML cleaning completed");
                return cleaned.Trim();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error cleaning HTML, using original");
                return htmlContent;
            }
        }

        /// <summary>
        /// Create complete HTML document with embedded CSS for medical forms
        /// </summary>
        private string CreateCompleteHtmlDocument(string bodyContent)
        {
            var css = GetMedicalFormCss();
            
            var completeHtml = $@"<!DOCTYPE html>
<html lang=""vi"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Sổ Khám Sức Khỏe Định Kỳ</title>
    <style>
        {css}
    </style>
</head>
<body>
    {bodyContent}
</body>
</html>";

            return completeHtml;
        }

        /// <summary>
        /// Get optimized CSS for medical forms in PDF - matching original HTML layout
        /// </summary>
        private string GetMedicalFormCss()
        {
            return @"
                body {
                    font-family: 'Times New Roman', Times, serif;
                    font-size: 14px;
                    background: #fff;
                    margin: 0;
                    padding: 20px;
                    color: #000;
                    line-height: 1.4;
                }
                
                .ksk-header {
                    text-align: center;
                    margin-bottom: 10px;
                }
                
                .ksk-header .quochoa {
                    font-weight: bold;
                    text-transform: uppercase;
                }
                
                .ksk-header .doclap {
                    font-style: italic;
                }
                
                .ksk-header .mau-so {
                    font-weight: bold;
                    margin-bottom: 5px;
                }
                
                .ksk-title {
                    text-align: center;
                    font-weight: bold;
                    text-transform: uppercase;
                    font-size: 18px;
                    margin-bottom: 10px;
                }
                
                .ksk-form {
                    max-width: 900px;
                    margin: 0 auto;
                    border: 2px solid #000;
                    padding: 24px 32px 16px 32px;
                    page-break-inside: avoid;
                }
                
                .ksk-row {
                    display: flex;
                    flex-direction: row;
                    gap: 32px;
                    margin-bottom: 20px;
                }
                
                .ksk-photo {
                    width: 120px;
                    min-width: 120px;
                    height: 160px;
                    border: 1px solid #000;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    font-size: 13px;
                    margin-bottom: 8px;
                }
                
                .ksk-photo img {
                    max-width: 100%;
                    max-height: 100%;
                    object-fit: cover;
                }
                
                .ksk-photo-label {
                    font-size: 12px;
                    text-align: center;
                    margin-top: 4px;
                }
                
                .ksk-info {
                    flex: 1;
                }
                
                .ksk-info-row {
                    display: flex;
                    margin-bottom: 4px;
                }
                
                .ksk-info-label {
                    min-width: 140px;
                    font-weight: bold;
                }
                
                .ksk-info-value {
                    flex: 1;
                    border-bottom: 1px dotted #333;
                    min-height: 20px;
                    padding-left: 4px;
                }
                
                .ksk-note {
                    font-size: 12px;
                    margin: 8px 0 12px 0;
                }
                
                .ksk-table-wrap {
                    margin: 12px 0;
                    overflow-x: auto;
                    -webkit-overflow-scrolling: touch;
                }
                
                .ksk-table {
                    width: 100%;
                    border-collapse: collapse;
                    margin: 12px 0;
                    font-size: 13px;
                }
                
                .ksk-table th, .ksk-table td {
                    border: 1px solid #000;
                    text-align: center;
                    padding: 4px 6px;
                }
                
                .ksk-table th {
                    background: #f8f8f8;
                    font-weight: bold;
                }
                
                .ksk-signature-row {
                    display: flex;
                    justify-content: space-between;
                    margin-top: 32px;
                }
                
                .ksk-signature-box {
                    width: 40%;
                    text-align: center;
                }
                
                .ksk-signature-box small {
                    font-size: 12px;
                }
                
                .ls-cell-small {
                    font-size: 12px;
                    padding: 2px 4px;
                    line-height: 1.2;
                }
                
                .ls-doctor-cell {
                    font-size: 11px;
                    text-align: center;
                    padding: 2px 2px;
                    line-height: 1.2;
                    min-width: 80px;
                }
                
                .ls-specialty {
                    font-style: italic;
                    font-weight: normal;
                }
                
                .ls-label {
                    font-size: 12px;
                    font-style: normal;
                    color: #333;
                }
                
                .font-bold {
                    font-weight: bold;
                }
                
                .font-normal {
                    font-weight: normal;
                }
                
                .italic {
                    font-style: italic;
                }
                
                .mb-2 {
                    margin-bottom: 8px;
                }
                
                .mb-4 {
                    margin-bottom: 16px;
                }
                
                .mb-6 {
                    margin-bottom: 24px;
                }
                
                .ml-4 {
                    margin-left: 16px;
                }
                
                .space-y-1 > * + * {
                    margin-top: 4px;
                }
                
                .text-sm {
                    font-size: 12px;
                }
                
                /* Page break controls */
                .page-break-before {
                    page-break-before: always;
                }
                
                .page-break-after {
                    page-break-after: always;
                }
                
                .avoid-break {
                    page-break-inside: avoid;
                }
                
                /* Page settings for PDF */
                @page {
                    size: A4;
                    margin: 1cm;
                }
            ";
        }

        /// <summary>
        /// Create fallback PDF when main generation fails
        /// </summary>
        private byte[] CreateFallbackPdf(string originalHtml, PdfSettings settings)
        {
            try
            {
                _logger.LogInformation("Creating simplified fallback PDF...");
                
                // Extract basic info from HTML
                var patientName = ExtractHtmlValue(originalHtml, "Họ và tên:");
                var gender = ExtractHtmlValue(originalHtml, "Giới tính:");
                var phone = ExtractHtmlValue(originalHtml, "Số điện thoại:");
                var workplace = ExtractHtmlValue(originalHtml, "Nơi công tác:");

                var fallbackHtml = $@"
<!DOCTYPE html>
<html lang=""vi"">
<head>
    <meta charset=""UTF-8"">
    <title>Sổ Khám Sức Khỏe</title>
    <style>
        body {{ 
            font-family: 'Times New Roman', serif; 
            font-size: 14px; 
            margin: 20px; 
            color: #000;
        }}
        .header {{ 
            text-align: center; 
            font-weight: bold; 
            margin-bottom: 30px; 
            border-bottom: 2px solid #000;
            padding-bottom: 15px;
        }}
        .title {{ 
            text-align: center; 
            font-size: 18px; 
            font-weight: bold; 
            margin: 20px 0; 
            text-transform: uppercase;
        }}
        .info-table {{ 
            width: 100%; 
            border-collapse: collapse; 
            margin: 20px 0; 
        }}
        .info-table td {{ 
            border: 1px solid #000; 
            padding: 10px; 
            vertical-align: top; 
        }}
        .info-label {{ 
            font-weight: bold; 
            background-color: #f5f5f5; 
            width: 30%; 
        }}
        .conclusion {{ 
            margin-top: 40px; 
            padding: 20px; 
            border: 2px solid #000; 
        }}
        .signature {{ 
            margin-top: 40px; 
            text-align: right; 
        }}
        .note {{ 
            margin-top: 30px; 
            text-align: center; 
            font-style: italic; 
            color: #666; 
            background-color: #f9f9f9;
            padding: 15px;
            border-radius: 5px;
        }}
    </style>
</head>
<body>
    <div class=""header"">
        <div>CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM</div>
        <div>Độc lập - Tự do - Hạnh phúc</div>
    </div>
    
    <div class=""title"">Sổ Khám Sức Khỏe Định Kỳ</div>
    
    <table class=""info-table"">
        <tr>
            <td class=""info-label"">Họ và tên:</td>
            <td>{patientName}</td>
        </tr>
        <tr>
            <td class=""info-label"">Giới tính:</td>
            <td>{gender}</td>
        </tr>
        <tr>
            <td class=""info-label"">Số điện thoại:</td>
            <td>{phone}</td>
        </tr>
        <tr>
            <td class=""info-label"">Nơi công tác:</td>
            <td>{workplace}</td>
        </tr>
        <tr>
            <td class=""info-label"">Ngày tạo PDF:</td>
            <td>{DateTime.Now:dd/MM/yyyy HH:mm}</td>
        </tr>
    </table>
    
    <div class=""conclusion"">
        <h3>KẾT LUẬN KHÁM SỨC KHỎE</h3>
        <p><strong>Phân loại sức khỏe:</strong> [Cần cập nhật từ kết quả khám]</p>
        <p><strong>Các bệnh, tật:</strong> [Cần cập nhật từ kết quả khám]</p>
        <p><strong>Đề nghị:</strong> [Cần cập nhật từ bác sĩ]</p>
    </div>
    
    <div class=""signature"">
        <p><strong>Bác sĩ khám</strong></p>
        <p>(Ký và đóng dấu)</p>
        <br><br><br>
        <p>_______________________</p>
    </div>
    
    <div class=""note"">
        <p><strong>Lưu ý:</strong> Đây là bản PDF đơn giản hóa do lỗi xử lý HTML phức tạp.</p>
        <p>Vui lòng liên hệ bộ phận kỹ thuật để nhận bản PDF đầy đủ.</p>
    </div>
</body>
</html>";

                using var memoryStream = new MemoryStream();
                var converterProperties = new ConverterProperties();
                converterProperties.SetCharset("UTF-8");

                HtmlConverter.ConvertToPdf(fallbackHtml, memoryStream, converterProperties);
                
                var pdfBytes = memoryStream.ToArray();
                _logger.LogInformation($"✅ Fallback PDF created successfully! Size: {pdfBytes.Length} bytes");
                
                return pdfBytes;
            }
            catch (Exception fallbackEx)
            {
                _logger.LogError(fallbackEx, "Even fallback PDF generation failed");
                throw new Exception("Không thể tạo PDF. Vui lòng thử lại sau.", fallbackEx);
            }
        }

        /// <summary>
        /// Extract value from HTML content
        /// </summary>
        private string ExtractHtmlValue(string html, string label)
        {
            try
            {
                var pattern = $@"{Regex.Escape(label)}\s*</div>\s*<div[^>]*>([^<]*)</div>";
                var match = Regex.Match(html, pattern, RegexOptions.IgnoreCase);
                
                if (match.Success)
                {
                    var value = match.Groups[1].Value.Trim();
                    return string.IsNullOrEmpty(value) ? "Chưa có thông tin" : value;
                }
                
                return "Chưa có thông tin";
            }
            catch
            {
                return "Chưa có thông tin";
            }
        }
    }
} 