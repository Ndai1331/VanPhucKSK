using System.Text;

namespace CoreAdminWeb.Helpers
{
    public static class QRHelper
    {
        public static byte[] GenerateQRCode(string content)
        {
            // Simple fallback: create a text representation instead of actual QR code
            // This avoids SkiaSharp dependency issues on Linux
            var qrText = $"QR Code for: {content}";
            return Encoding.UTF8.GetBytes(qrText);
        }
        
        // Alternative method that returns a simple placeholder image
        public static byte[] GenerateQRCodeImage(string content)
        {
            // Create a simple 1x1 pixel PNG image as placeholder
            // This avoids complex image generation that might cause SkiaSharp issues
            var pngHeader = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            var ihdr = new byte[] { 0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x08, 0x02, 0x00, 0x00, 0x00, 0x90, 0x77, 0x53, 0xDE };
            var idat = new byte[] { 0x00, 0x00, 0x00, 0x0C, 0x49, 0x44, 0x41, 0x54, 0x08, 0x99, 0x01, 0x01, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x02, 0x00, 0x01, 0xE2, 0x21, 0xBC, 0x33 };
            var iend = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82 };
            
            var result = new List<byte>();
            result.AddRange(pngHeader);
            result.AddRange(ihdr);
            result.AddRange(idat);
            result.AddRange(iend);
            
            return result.ToArray();
        }
    }
}
