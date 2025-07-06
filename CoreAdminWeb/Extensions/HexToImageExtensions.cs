using System;
using System.Text;
using System.IO;

namespace CoreAdminWeb.Extensions
{
    /// <summary>
    /// Extension methods for converting hex string to image format
    /// </summary>
    public static class HexToImageExtensions
    {
        /// <summary>
        /// Convert hex string to base64 data URL for image display
        /// </summary>
        /// <param name="hexString">Hex string starting with 0x or just hex digits</param>
        /// <param name="imageFormat">Image format (default: png)</param>
        /// <returns>Base64 data URL string</returns>
        public static string ToBase64Image(this string hexString, string imageFormat = "png")
        {
            if (string.IsNullOrEmpty(hexString))
                return string.Empty;

            try
            {
                // Remove 0x prefix if present
                string cleanHex = hexString.StartsWith("0x", StringComparison.OrdinalIgnoreCase) 
                    ? hexString.Substring(2) 
                    : hexString;

                // Convert hex to byte array
                byte[] bytes = HexStringToByteArray(cleanHex);
                
                // Convert to base64
                string base64 = Convert.ToBase64String(bytes);
                
                // Return as data URL
                return $"data:image/{imageFormat};base64,{base64}";
            }
            catch (Exception)
            {
                // Return empty string if conversion fails
                return string.Empty;
            }
        }

        /// <summary>
        /// Check if string is a valid hex signature (starts with PNG/JPEG magic number)
        /// </summary>
        /// <param name="hexString">Hex string to check</param>
        /// <returns>True if valid hex signature</returns>
        public static bool IsValidHexSignature(this string hexString)
        {
            if (string.IsNullOrEmpty(hexString))
                return false;

            try
            {
                string cleanHex = hexString.StartsWith("0x", StringComparison.OrdinalIgnoreCase) 
                    ? hexString.Substring(2) 
                    : hexString;

                // Check minimum length (at least 16 characters for magic number)
                if (cleanHex.Length < 16)
                    return false;

                // Check if it's valid hex
                for (int i = 0; i < Math.Min(16, cleanHex.Length); i++)
                {
                    if (!IsHexChar(cleanHex[i]))
                        return false;
                }

                // Check for common image magic numbers
                string magicNumber = cleanHex.Substring(0, 16).ToUpper();
                
                // PNG magic number: 89504E470D0A1A0A
                if (magicNumber.StartsWith("89504E47"))
                    return true;
                
                // JPEG magic number: FFD8FF
                if (magicNumber.StartsWith("FFD8FF"))
                    return true;

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Convert hex string to byte array
        /// </summary>
        /// <param name="hex">Hex string</param>
        /// <returns>Byte array</returns>
        private static byte[] HexStringToByteArray(string hex)
        {
            if (hex.Length % 2 != 0)
                throw new ArgumentException("Hex string must have an even number of characters");

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }

        /// <summary>
        /// Check if character is a valid hex character
        /// </summary>
        /// <param name="c">Character to check</param>
        /// <returns>True if valid hex character</returns>
        private static bool IsHexChar(char c)
        {
            return (c >= '0' && c <= '9') || 
                   (c >= 'A' && c <= 'F') || 
                   (c >= 'a' && c <= 'f');
        }

        /// <summary>
        /// Save base64 image data to wwwroot/images folder
        /// </summary>
        /// <param name="base64Data">Base64 image data (with or without data:image prefix)</param>
        /// <param name="fileName">File name without extension</param>
        /// <param name="webRootPath">Web root path (from IWebHostEnvironment)</param>
        /// <returns>Relative path to saved image or empty string if failed</returns>
        public static string SaveBase64AsImage(string base64Data, string fileName, string webRootPath)
        {
            if (string.IsNullOrEmpty(base64Data) || string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(webRootPath))
                return string.Empty;

            try
            {
                // Remove data:image prefix if present
                string cleanBase64 = base64Data;
                if (base64Data.StartsWith("data:image"))
                {
                    var commaIndex = base64Data.IndexOf(',');
                    if (commaIndex > 0)
                        cleanBase64 = base64Data.Substring(commaIndex + 1);
                }

                // Convert base64 to byte array
                byte[] imageBytes = Convert.FromBase64String(cleanBase64);

                // Determine image format
                string extension = GetImageExtensionFromBytes(imageBytes);
                if (string.IsNullOrEmpty(extension))
                    extension = "png"; // Default fallback

                // Create images directory if not exists
                string imagesDir = webRootPath;
                if (!Directory.Exists(imagesDir))
                    Directory.CreateDirectory(imagesDir);

                // Generate file path (with extension)
                string safeFileName = MakeSafeFileName(fileName);
                string fullFileName = $"{safeFileName}.{extension}";
                string filePath = Path.Combine(imagesDir, fullFileName);

                // Save file (overwrite if exists)
                File.WriteAllBytes(filePath, imageBytes);

                // Calculate correct relative path for web use
                // Find where 'wwwroot' ends in the webRootPath
                int wwwrootIndex = webRootPath.IndexOf("wwwroot");
                if (wwwrootIndex >= 0)
                {
                    // Get the part after wwwroot
                    string afterWwwroot = webRootPath.Substring(wwwrootIndex + 7); // 7 is length of "wwwroot"
                    // Convert to web path format and add filename
                    string webPath = afterWwwroot.Replace('\\', '/');
                    if (!webPath.StartsWith("/")) webPath = "/" + webPath;
                    if (!webPath.EndsWith("/")) webPath += "/";
                    return webPath + fullFileName;
                }
                
                // Fallback - assumes standard structure
                return $"/images/{fullFileName}";
            }
            catch (Exception ex)
            {
                // Log error if needed
                Console.WriteLine($"Error saving image {fileName}: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Get image file extension from byte array
        /// </summary>
        /// <param name="bytes">Image byte array</param>
        /// <returns>File extension without dot</returns>
        private static string GetImageExtensionFromBytes(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 4)
                return "png";

            // PNG signature: 89 50 4E 47
            if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
                return "png";

            // JPEG signature: FF D8 FF
            if (bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
                return "jpg";

            // GIF signature: 47 49 46
            if (bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46)
                return "gif";

            // Default to PNG
            return "png";
        }

        /// <summary>
        /// Make file name safe for file system
        /// </summary>
        /// <param name="fileName">Original file name</param>
        /// <returns>Safe file name</returns>
        private static string MakeSafeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return $"signature_{DateTime.Now:yyyyMMdd_HHmmss}";

            // Remove invalid characters
            char[] invalidChars = Path.GetInvalidFileNameChars();
            string safeName = fileName;
            
            foreach (char invalidChar in invalidChars)
            {
                safeName = safeName.Replace(invalidChar, '_');
            }

            // Replace spaces and special characters
            safeName = safeName.Replace(' ', '_')
                              .Replace('?', '_')
                              .Replace('&', '_')
                              .Replace('=', '_');

            // Limit length
            if (safeName.Length > 50)
                safeName = safeName.Substring(0, 50);

            return safeName;
        }

        public static string GetSignatureDisplayHtml(this string signatureData, 
            string? fallbackText = "", 
            string? fileName="",
            int maxWidth = 120, 
            int maxHeight = 60,
            string? webRootPath = null,
            string? baseUrl = null)
        {
            if (string.IsNullOrEmpty(signatureData))
                return $"<span class='signature-text'>{fallbackText}</span>";


            string base64Image = "";
            // Check if it's a hex signature
            if (signatureData.IsValidHexSignature())
            {
                 base64Image = signatureData.ToBase64Image();
               
            }
            else
            {
                base64Image = signatureData;
            }
            
            
            if (!string.IsNullOrEmpty(base64Image))
            {
                if (!string.IsNullOrEmpty(webRootPath) && !string.IsNullOrEmpty(fileName))
                {
                    var imagePath = SaveBase64AsImage(base64Image, fileName, webRootPath);
                    if (!string.IsNullOrEmpty(imagePath))
                    {
                        return $"<div class='signature-wrapper'><img src='{baseUrl}{imagePath}' alt='Chữ ký' class='signature-image' style='max-width:{maxWidth}px; max-height:{maxHeight}px; object-fit: contain;' /></div>";
                    }
                }
            }

            // Fallback to text display
            return $"<span class='signature-text'>{signatureData}</span>";
        }

        /// <summary>
        /// Get optimized signature display HTML for PDF export
        /// </summary>
        /// <param name="signatureData">Signature data (hex string or text)</param>
        /// <param name="fallbackText">Fallback text if not hex</param>
        /// <param name="maxWidth">Maximum width for signature image</param>
        /// <param name="maxHeight">Maximum height for signature image</param>
        /// <returns>HTML string for signature display optimized for PDF</returns>
        public static string GetOptimizedSignatureDisplayHtml(this string signatureData, 
            string? fallbackText = "", 
            int maxWidth = 120, 
            int maxHeight = 60)
        {
            if (string.IsNullOrEmpty(signatureData))
                return $"<span class='signature-text'>{fallbackText}</span>";

            // Check if it's a hex signature
            if (signatureData.IsValidHexSignature())
            {
                var base64Image = signatureData.ToBase64Image();
                if (!string.IsNullOrEmpty(base64Image))
                {
                    // Cho PDF, luôn sử dụng placeholder nếu ảnh quá lớn
                    if (base64Image.Length > 30000) // Ngưỡng thấp hơn cho PDF
                    {
                        return $"<div class='signature-placeholder' style='border: 1px solid #000; padding: 4px; text-align: center; width: {maxWidth}px; height: {maxHeight}px; display: inline-block; line-height: {maxHeight}px; font-size: 12px;'>Chữ ký</div>";
                    }
                    
                    return $"<img src='{base64Image}' alt='Chữ ký' class='signature-image' style='max-width:{maxWidth}px; max-height:{maxHeight}px; object-fit: contain;' />";
                }
            }

            // Fallback to text display
            return $"<span class='signature-text'>{signatureData}</span>";
        }
    }
} 