using iText.Html2pdf;
using iText.Html2pdf.Resolver.Font;
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

                // Check if HTML is already a complete document
                var completeHtml = IsCompleteHtmlDocument(cleanHtml)
                    ? cleanHtml
                    : CreateCompleteHtmlDocument(cleanHtml);
                _logger.LogInformation($"Complete HTML size: {completeHtml.Length} characters");

                // Generate PDF using iText7
                using var memoryStream = new MemoryStream();

                var converterProperties = new ConverterProperties();

                // Configure font provider for Vietnamese support
                var fontProvider = new DefaultFontProvider(true, true, true);
                fontProvider.AddFont("wwwroot/assets/fonts/times.ttf");

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
        /// Check if HTML is already a complete document
        /// </summary>
        private bool IsCompleteHtmlDocument(string htmlContent)
        {
            if (string.IsNullOrEmpty(htmlContent))
            {
                return false;
            }

            var trimmed = htmlContent.Trim();
            return trimmed.StartsWith("<!DOCTYPE html", StringComparison.OrdinalIgnoreCase) &&
                   trimmed.Contains("<html", StringComparison.OrdinalIgnoreCase) &&
                   trimmed.Contains("<head", StringComparison.OrdinalIgnoreCase) &&
                   trimmed.Contains("<body", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Clean HTML content for PDF generation
        /// </summary>
        private string CleanHtmlForPdf(string htmlContent)
        {
            if (string.IsNullOrEmpty(htmlContent))
            {
                return htmlContent;
            }

            var minified = htmlContent;

            try
            {
                // Bước 1: Xóa comment HTML (trừ comment điều kiện của IE)
                minified = Regex.Replace(minified, @"<!--(?!<!\[if).*?-->", "");

                // Bước 2: Minify CSS trong thẻ <style>
                minified = Regex.Replace(minified, @"<style\b[^>]*>(.*?)</style>", match =>
                {
                    string css = match.Groups[1].Value;
                    return "<style>" + MinifyCss(css) + "</style>";
                }, RegexOptions.Singleline);

                // Bước 3: Minify JS trong thẻ <script>
                minified = Regex.Replace(minified, @"<script\b[^>]*>(.*?)</script>", match =>
                {
                    string js = match.Groups[1].Value;
                    return "<script>" + MinifyJs(js) + "</script>";
                }, RegexOptions.Singleline);

                // Bước 4: Xóa khoảng trắng thừa giữa các thẻ
                minified = Regex.Replace(minified, @">\s+<", "><");

                // Bước 5: Xóa khoảng trắng đầu/cuối dòng
                minified = Regex.Replace(minified, @"^\s+|\s+$", "", RegexOptions.Multiline);

                // Bước 6: Xóa khoảng trắng thừa trong nội dung (giữ nguyên trong thẻ pre)
                minified = Regex.Replace(minified, @"(?<!<pre\b[^>]*>)\s{2,}", " ");

                // Bước 7: Xóa dòng trống
                minified = Regex.Replace(minified, @"\n\s*\n", "\n");

                _logger.LogInformation("HTML minification completed");
                return minified;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error minifying HTML, returning original");
                return htmlContent;
            }
        }

        static string MinifyCss(string css)
        {
            // Xóa comment CSS
            css = Regex.Replace(css, @"/\*.*?\*/", "");

            // Xóa khoảng trắng thừa
            css = Regex.Replace(css, @"\s*([:;{}])\s*", "$1");

            // Xóa khoảng trắng đầu/cuối
            css = css.Trim();

            // Xóa khoảng trắng thừa giữa các giá trị
            css = Regex.Replace(css, @"\s{2,}", " ");

            return css;
        }

        static string MinifyJs(string js)
        {
            // Xóa comment dòng đơn (//) nhưng giữ nguyên URL (http://, https://)
            js = Regex.Replace(js, @"(?<![a-zA-Z0-9/:])//.*$", "", RegexOptions.Multiline);

            // Xóa comment khối (/* */)
            js = Regex.Replace(js, @"/\*.*?\*/", "", RegexOptions.Singleline);

            // Xóa khoảng trắng thừa
            js = Regex.Replace(js, @"\s*([=+\-*/;{}()])\s*", "$1");

            // Xóa khoảng trắng đầu/cuối
            js = js.Trim();

            // Xóa khoảng trắng thừa giữa các từ
            js = Regex.Replace(js, @"\s{2,}", " ");

            return js;
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
               
                .ksk-header-container {
                    max-width: 900px !important;
                    margin: 0 auto !important;
                    display: flex !important;
                    flex-direction: row !important;
                    align-items: center !important;
                    margin-bottom: 10px !important;
                    gap: 15px !important;
                }
                
                .ksk-header-logo {
                    flex-shrink: 0 !important;
                }
                
                .header-logo {
                        width: 150px !important;
                        height: 150px !important;
                        object-fit: contain !important;
                    }
                
                .ksk-header {
                    flex: 1 !important;
                    text-align: center !important;
                    margin-bottom: 5px !important;
                    line-height: 1.1 !important;
                }
                
                .ksk-header .quochoa {
                    font-weight: bold !important;
                    text-transform: uppercase !important;
                    font-size: 14px !important;
                }
                
                .ksk-header .doclap {
                    font-style: italic !important;
                    font-size: 13px !important;
                }
                
                .ksk-header .mau-so {
                    font-weight: bold !important;
                    margin-bottom: 2px !important;
                    font-size: 12px !important;
                }
                
                .ksk-title {
                    text-align: center !important;
                    font-weight: bold !important;
                    text-transform: uppercase !important;
                    font-size: 16px !important;
                    margin: 5px 0 10px 0 !important;
                    line-height: 1.1 !important;
                }
                
                .ksk-form {
                    max-width: calc(100% - 10px) !important;
                    width: 100% !important;
                    margin: 0 auto !important;
                    border-top: 2px solid #000 !important;
                    border-right: 2px solid #000 !important;
                    border-bottom: 2px solid #000 !important;
                    border-left: 2px solid #000 !important;
                    padding: 20px 30px 16px 30px !important;
                    page-break-inside: avoid !important;
                    box-sizing: border-box !important;
                    overflow: hidden !important;
                }
                
                .ksk-row {
                    display: flex !important;
                    flex-direction: row !important;
                    gap: 30px !important;
                    margin-bottom: 16px !important;
                    max-width: 100% !important;
                    overflow: hidden !important;
                    box-sizing: border-box !important;
                }
                
                .ksk-photo {
                    width: 90px !important;
                    min-width: 90px !important;
                    height: 120px !important;
                    border: 1px solid #000 !important;
                    display: flex !important;
                    align-items: center !important;
                    justify-content: center !important;
                    font-size: 11px !important;
                    margin-bottom: 6px !important;
                }
                
                .ksk-photo img {
                    max-width: 100% !important;
                    max-height: 100% !important;
                    object-fit: cover !important;
                }
                
                .ksk-photo-label {
                    font-size: 10px !important;
                    text-align: center !important;
                    margin-top: 4px !important;
                }
                
                .ksk-info {
                    flex: 1 !important;
                    max-width: calc(100% - 150px) !important;
                }
                
                .ksk-info-row {
                    display: flex !important;
                    margin-bottom: 4px !important;
                    max-width: 100% !important;
                    overflow: visible !important;
                    align-items: flex-start !important;
                    flex-wrap: nowrap !important;
                }
                
                .ksk-info-label {
                    min-width: 100px !important;
                    font-weight: bold !important;
                    flex-shrink: 0 !important;
                    font-size: 13px !important;
                    padding-right: 8px !important;
                    padding-top: 2px !important;
                    vertical-align: top !important;
                }
                
                .ksk-info-value {
                    flex: 1 !important;
                    border-bottom: 1px dotted #333 !important;
                    min-height: 20px !important;
                    padding-left: 4px !important;
                }
                
                .ksk-note {
                    font-size: 11px !important;
                    margin: 12px 0 16px 0 !important;
                    padding: 8px !important;
                    border: 1px solid #333 !important;
                    background-color: #f9f9f9 !important;
                    line-height: 1.3 !important;
                }
                
                .ksk-table-wrap {
                    margin: 16px 0 !important;
                    overflow-x: auto !important;
                    -webkit-overflow-scrolling: touch !important;
                }
                
                .ksk-table {
                    width: 100% !important;
                    border-collapse: separate !important;
                    border-spacing: 0 !important;
                    margin: 12px 0 !important;
                    font-size: 12px !important;
                    border: 1px solid #000 !important;
                }
                
                .ksk-table th, .ksk-table td {
                    border-right: 1px solid #000 !important;
                    border-bottom: 1px solid #000 !important;
                    text-align: center !important;
                    padding: 3px 4px !important;
                    vertical-align: middle !important;
                }
                
                .ksk-table th:last-child, .ksk-table td:last-child {
                    border-right: none !important;
                }
                
                .ksk-table tr:last-child th, .ksk-table tr:last-child td {
                    border-bottom: none !important;
                }
                
                .ksk-table th {
                    background: #f8f8f8 !important;
                    font-weight: bold !important;
                    border-top: none !important;
                }
                
                .ksk-table tr:first-child th {
                    border-top: none !important;
                }
                
                .ksk-table tr:first-child td {
                    border-top: none !important;
                }
                
                .ksk-signature-row {
                    display: flex !important;
                    justify-content: space-between !important;
                    margin-top: 24px !important;
                    gap: 20px !important;
                }
                
                .ksk-signature-box {
                    width: 45% !important;
                    text-align: center !important;
                    font-size: 12px !important;
                    padding: 10px !important;
                    line-height: 1.4 !important;
                }
                
                .ksk-signature-box small {
                    font-size: 10px !important;
                    font-style: italic !important;
                }
                
                .ls-cell-small {
                    font-size: 12px !important;
                    padding: 2px 4px !important;
                    line-height: 1.2 !important;
                    border-right: 1px solid #000 !important;
                    border-bottom: 1px solid #000 !important;
                }
                
                .ls-doctor-cell {
                    font-size: 11px !important;
                    text-align: center !important;
                    padding: 2px 2px !important;
                    line-height: 1.2 !important;
                    min-width: 80px !important;
                    border-right: 1px solid #000 !important;
                    border-bottom: 1px solid #000 !important;
                }
                
                .ls-specialty {
                    font-style: italic !important;
                    font-weight: normal !important;
                    text-align: left !important;
                    padding-left: 8px !important;
                }
                
                .ls-label {
                    font-size: 12px !important;    
                    font-style: normal !important;
                    color: #333 !important;
                    text-align: left !important;
                    padding-left: 16px !important;
                }
                
                .font-bold {
                    font-weight: bold !important;
                }
                
                .font-normal {
                    font-weight: normal !important;
                }
                
                .italic {
                    font-style: italic !important;
                }
                
                .mb-2 {
                    margin-bottom: 6px !important;
                }
                
                .mb-4 {
                    margin-bottom: 12px !important;
                }
                
                .mb-6 {
                    margin-bottom: 18px !important;
                }
                
                .ml-4 {
                    margin-left: 12px !important;
                }
                
                .space-y-1 > * + * {
                    margin-top: 3px !important;
                }
                
                .text-sm {
                    font-size: 11px !important;
                }
                
                /* Inline text elements for date/time fields */
                .ksk-info-row span {
                    white-space: nowrap !important;
                    margin: 0 4px !important;
                    flex-shrink: 0 !important;
                    font-size: 13px !important;
                    font-weight: normal !important;
                }
                
                /* Special styling for date input fields */
                .ksk-info-row .ksk-info-value {
                    margin-right: 4px !important;
                }
                
                .ksk-info-value + span + .ksk-info-value {
                    min-width: 60px !important;
                    max-width: 80px !important;
                    text-align: center !important;
                    white-space: nowrap !important;
                }
                
                /* Specific styling for birth date row (mục 3) */
                .birth-date-row .ksk-info-value {
                    max-width: 80px !important;
                    flex: 0 0 auto !important;
                }
                
                .birth-date-row span {
                    margin: 0 8px !important;
                    font-size: 13px !important;
                }
                
                .issued-date-row .ksk-info-value:first-of-type {
                    max-width: 100px !important;
                    flex: 0 0 auto !important;
                }
                

                
                /* Page break controls */
                .page-break-before {
                    page-break-before: always !important;
                }
                
                .page-break-after {
                    page-break-after: always !important;
                }
                
                .avoid-break {
                    page-break-inside: avoid !important;
                }
                
                /* Page settings for PDF - compact margins */
                @page {
                    size: A4 !important;
                    margin: 0.8cm 0.6cm !important;
                }
                
                /* Ensure proper border rendering */
                * {
                    box-sizing: border-box !important;
                }
                
                /* Force border visibility for iText7 */
                .ksk-form, .ksk-table, .ksk-photo {
                    -webkit-print-color-adjust: exact !important;
                    color-adjust: exact !important;
                }
                
                /* iText7 border fix - ensure all borders render */
                .ksk-form {
                    outline: 2px solid #000 !important;
                    outline-offset: -2px !important;
                }
                
                /* Alternative border approach for better iText7 compatibility */
                .ksk-form::before {
                    content: '' !important;
                    position: absolute !important;
                    top: 0 !important;
                    right: 0 !important;
                    bottom: 0 !important;
                    left: 0 !important;
                    border: 2px solid #000 !important;
                    pointer-events: none !important;
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