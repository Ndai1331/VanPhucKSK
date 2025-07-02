namespace CoreAdminWeb.Services.PDFService
{
    /// <summary>
    /// Interface for PDF generation services
    /// </summary>
    public interface IPdfService
    {
        /// <summary>
        /// Generate PDF from HTML content
        /// </summary>
        /// <param name="htmlContent">HTML content to convert</param>
        /// <param name="fileName">PDF file name</param>
        /// <returns>PDF as byte array</returns>
        byte[] GeneratePdfFromHtml(string htmlContent, string fileName = "document.pdf");

        /// <summary>
        /// Generate PDF from HTML content with custom settings
        /// </summary>
        /// <param name="htmlContent">HTML content to convert</param>
        /// <param name="settings">PDF generation settings</param>
        /// <returns>PDF as byte array</returns>
        byte[] GeneratePdfFromHtml(string htmlContent, PdfSettings settings);
    }

    /// <summary>
    /// PDF generation settings
    /// </summary>
    public class PdfSettings
    {
        public string FileName { get; set; } = "document.pdf";
        public string PageSize { get; set; } = "A4";
        public string Orientation { get; set; } = "Portrait";
        public int MarginTop { get; set; } = 10;
        public int MarginBottom { get; set; } = 10;
        public int MarginLeft { get; set; } = 10;
        public int MarginRight { get; set; } = 10;
        public string? HeaderHtml { get; set; }
        public string? FooterHtml { get; set; }
    }
} 