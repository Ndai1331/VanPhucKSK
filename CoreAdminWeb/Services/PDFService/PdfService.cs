using DinkToPdf;
using DinkToPdf.Contracts;

namespace CoreAdminWeb.Services.PDFService
{
    /// <summary>
    /// PDF generation service using DinkToPdf
    /// </summary>
    public class PdfService : IPdfService
    {
        private readonly IConverter _converter;

        public PdfService(IConverter converter)
        {
            _converter = converter;
        }

        /// <summary>
        /// Generate PDF from HTML content with default settings
        /// </summary>
        public byte[] GeneratePdfFromHtml(string htmlContent, string fileName = "document.pdf")
        {
            var settings = new PdfSettings { FileName = fileName };
            return GeneratePdfFromHtml(htmlContent, settings);
        }

        /// <summary>
        /// Generate PDF from HTML content with custom settings
        /// </summary>
        public byte[] GeneratePdfFromHtml(string htmlContent, PdfSettings settings)
        {
            try
            {
                var globalSettings = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    Orientation = settings.Orientation.ToLower() == "landscape" ? Orientation.Landscape : Orientation.Portrait,
                    PaperSize = GetPaperSize(settings.PageSize),
                    Margins = new MarginSettings
                    {
                        Top = settings.MarginTop,
                        Bottom = settings.MarginBottom,
                        Left = settings.MarginLeft,
                        Right = settings.MarginRight,
                        Unit = Unit.Millimeters
                    }
                };

                var objectSettings = new ObjectSettings
                {
                    PagesCount = true,
                    HtmlContent = htmlContent,
                    WebSettings = new WebSettings
                    {
                        DefaultEncoding = "utf-8"
                    },
                    FooterSettings = new FooterSettings
                    {
                        FontName = "Arial",
                        FontSize = 9,
                        Right = "Trang [page] / [toPage]",
                        Line = true
                    }
                };

                var doc = new HtmlToPdfDocument()
                {
                    GlobalSettings = globalSettings,
                    Objects = { objectSettings }
                };

                return _converter.Convert(doc);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tạo PDF: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get paper size from string
        /// </summary>
        private PaperKind GetPaperSize(string pageSize)
        {
            return pageSize.ToUpper() switch
            {
                "A3" => PaperKind.A3,
                "A4" => PaperKind.A4,
                "A5" => PaperKind.A5,
                "LETTER" => PaperKind.Letter,
                "LEGAL" => PaperKind.Legal,
                _ => PaperKind.A4
            };
        }

        /// <summary>
        /// Get custom CSS for PDF styling
        /// </summary>
        private string GetCustomCSS()
        {
            return @"
                @page {
                    size: A4;
                    margin: 1cm;
                }
                body {
                    font-family: 'Times New Roman', serif;
                    font-size: 12px;
                    line-height: 1.4;
                    color: #000;
                    background: white;
                }
                .no-print {
                    display: none !important;
                }
                .ksk-form {
                    border: 2px solid #000;
                    padding: 20px;
                    margin: 0;
                    max-width: none;
                }
                .ksk-table {
                    border-collapse: collapse;
                    width: 100%;
                    font-size: 11px;
                }
                .ksk-table th,
                .ksk-table td {
                    border: 1px solid #000;
                    padding: 4px;
                    text-align: center;
                }
                .ksk-header {
                    text-align: center;
                    margin-bottom: 10px;
                    font-size: 12px;
                }
                .ksk-title {
                    text-align: center;
                    font-weight: bold;
                    font-size: 16px;
                    margin: 10px 0;
                }
                .ksk-signature-row {
                    display: flex;
                    justify-content: space-between;
                    margin-top: 20px;
                }
                .ksk-signature-box {
                    width: 45%;
                    text-align: center;
                }
            ";
        }
    }
} 