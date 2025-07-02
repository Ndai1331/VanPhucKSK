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
                    padding: 5px;
                    color: #000;
                    line-height: 1.2;
                    width: 100%;
                    box-sizing: border-box;
                }
               
                .ksk-header {
                    text-align: center;
                    margin-bottom: 5px;
                    line-height: 1.1;
                }
                
                .ksk-header .quochoa {
                    font-weight: bold;
                    text-transform: uppercase;
                    font-size: 14px;
                }
                
                .ksk-header .doclap {
                    font-style: italic;
                    font-size: 13px;
                }
                
                .ksk-header .mau-so {
                    font-weight: bold;
                    margin-bottom: 2px;
                    font-size: 12px;
                }
                
                .ksk-title {
                    text-align: center;
                    font-weight: bold;
                    text-transform: uppercase;
                    font-size: 16px;
                    margin: 5px 0 10px 0;
                    line-height: 1.1;
                }
                
                .ksk-form {
                    max-width: calc(100% - 10px);
                    width: 100%;
                    margin: 0 auto;
                    border-top: 2px solid #000;
                    border-right: 2px solid #000;
                    border-bottom: 2px solid #000;
                    border-left: 2px solid #000;
                    padding: 20px 30px 16px 30px;
                    page-break-inside: avoid;
                    box-sizing: border-box;
                    overflow: hidden;
                }
                
                .ksk-row {
                    display: flex;
                    flex-direction: row;
                    gap: 30px;
                    margin-bottom: 16px;
                    max-width: 100%;
                    overflow: hidden;
                    box-sizing: border-box;
                }
                
                .ksk-photo {
                    width: 90px;
                    min-width: 90px;
                    height: 120px;
                    border: 1px solid #000;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    font-size: 11px;
                    margin-bottom: 6px;
                }
                
                .ksk-photo img {
                    max-width: 100%;
                    max-height: 100%;
                    object-fit: cover;
                }
                
                .ksk-photo-label {
                    font-size: 10px;
                    text-align: center;
                    margin-top: 4px;
                }
                
                .ksk-info {
                    flex: 1;
                    max-width: calc(100% - 150px);
                }
                
                .ksk-info-row {
                    display: flex;
                    margin-bottom: 4px;
                    max-width: 100%;
                    overflow: visible;
                    align-items: flex-start;
                    flex-wrap: nowrap;
                }
                
                .ksk-info-label {
                    min-width: 100px;
                    font-weight: bold;
                    flex-shrink: 0;
                    font-size: 13px;
                    padding-right: 8px;
                    padding-top: 2px;
                    vertical-align: top;
                }
                
                .ksk-info-value {
                    flex: 1;
                    border-bottom: 1px dotted #333;
                    min-height: 20px;
                    padding-left: 4px;
                }
                
                .ksk-note {
                    font-size: 11px;
                    margin: 12px 0 16px 0;
                    padding: 8px;
                    border: 1px solid #333;
                    background-color: #f9f9f9;
                    line-height: 1.3;
                }
                
                .ksk-table-wrap {
                    margin: 16px 0;
                    overflow-x: auto;
                    -webkit-overflow-scrolling: touch;
                }
                
                .ksk-table {
                    width: 100%;
                    border-collapse: separate;
                    border-spacing: 0;
                    margin: 12px 0;
                    font-size: 12px;
                    border: 1px solid #000;
                }
                
                .ksk-table th, .ksk-table td {
                    border-right: 1px solid #000;
                    border-bottom: 1px solid #000;
                    text-align: center;
                    padding: 3px 4px;
                    vertical-align: middle;
                }
                
                .ksk-table th:last-child, .ksk-table td:last-child {
                    border-right: none;
                }
                
                .ksk-table tr:last-child th, .ksk-table tr:last-child td {
                    border-bottom: none;
                }
                
                .ksk-table th {
                    background: #f8f8f8;
                    font-weight: bold;
                    border-top: none;
                }
                
                .ksk-table tr:first-child th {
                    border-top: none;
                }
                
                .ksk-table tr:first-child td {
                    border-top: none;
                }
                
                .ksk-signature-row {
                    display: flex;
                    justify-content: space-between;
                    margin-top: 24px;
                    gap: 20px;
                }
                
                .ksk-signature-box {
                    width: 45%;
                    text-align: center;
                    font-size: 12px;
                    padding: 10px;
                    line-height: 1.4;
                }
                
                .ksk-signature-box small {
                    font-size: 10px;
                    font-style: italic;
                }
                
                .ls-cell-small {
                    font-size: 12px;
                    padding: 2px 4px;
                    line-height: 1.2;
                    border-right: 1px solid #000 !important;
                    border-bottom: 1px solid #000 !important;
                }
                
                .ls-doctor-cell {
                    font-size: 11px;
                    text-align: center;
                    padding: 2px 2px;
                    line-height: 1.2;
                    min-width: 80px;
                    border-right: 1px solid #000 !important;
                    border-bottom: 1px solid #000 !important;
                }
                
                .ls-specialty {
                    font-style: italic;
                    font-weight: normal;
                    text-align: left;
                    padding-left: 8px;
                }
                
                .ls-label {
                    font-size: 12px;
                    font-style: normal;
                    color: #333;
                    text-align: left;
                    padding-left: 16px;
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
                    margin-bottom: 6px;
                }
                
                .mb-4 {
                    margin-bottom: 12px;
                }
                
                .mb-6 {
                    margin-bottom: 18px;
                }
                
                .ml-4 {
                    margin-left: 12px;
                }
                
                .space-y-1 > * + * {
                    margin-top: 3px;
                }
                
                .text-sm {
                    font-size: 11px;
                }
                
                /* Inline text elements for date/time fields */
                .ksk-info-row span {
                    white-space: nowrap;
                    margin: 0 4px;
                    flex-shrink: 0;
                    font-size: 13px;
                    font-weight: normal;
                }
                
                /* Special styling for date input fields */
                .ksk-info-row .ksk-info-value {
                    margin-right: 4px;
                }
                
                .ksk-info-value + span + .ksk-info-value {
                    min-width: 60px;
                    max-width: 80px;
                    text-align: center;
                    white-space: nowrap;
                }
                
                /* Specific styling for birth date row (mục 3) */
                .birth-date-row .ksk-info-value {
                    max-width: 80px;
                    flex: 0 0 auto;
                }
                
                .birth-date-row span {
                    margin: 0 8px;
                    font-size: 13px;
                }
                
                .issued-date-row .ksk-info-value:first-of-type {
                    max-width: 100px;
                    flex: 0 0 auto;
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
                
                /* Page settings for PDF - compact margins */
                @page {
                    size: A4;
                    margin: 0.8cm 0.6cm;
                }
                
                /* Ensure proper border rendering */
                * {
                    box-sizing: border-box;
                }
                
                /* Force border visibility for iText7 */
                .ksk-form, .ksk-table, .ksk-photo {
                    -webkit-print-color-adjust: exact;
                    color-adjust: exact;
                }
                
                /* iText7 border fix - ensure all borders render */
                .ksk-form {
                    outline: 2px solid #000;
                    outline-offset: -2px;
                }
                
                /* Alternative border approach for better iText7 compatibility */
                .ksk-form::before {
                    content: '';
                    position: absolute;
                    top: 0;
                    right: 0;
                    bottom: 0;
                    left: 0;
                    border: 2px solid #000;
                    pointer-events: none;
                }

                .30px {
                    width: 30px !important;
                }
                .70px {
                    width: 70px !important;
                }
                .50px {
                    width: 50px !important;
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