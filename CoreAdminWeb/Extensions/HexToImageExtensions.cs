using System;
using System.Text;

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
        /// Get signature display HTML - either image or text
        /// </summary>
        /// <param name="signatureData">Signature data (hex string or text)</param>
        /// <param name="fallbackText">Fallback text if not hex</param>
        /// <param name="maxWidth">Maximum width for signature image</param>
        /// <param name="maxHeight">Maximum height for signature image</param>
        /// <returns>HTML string for signature display</returns>
        public static string GetSignatureDisplayHtml(this string signatureData, 
            string fallbackText = "", 
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
                    return $"<img src='{base64Image}' alt='Chữ ký' class='signature-image' style='max-width:{maxWidth}px; max-height:{maxHeight}px; object-fit: contain;' />";
                }
            }

            // Fallback to text display
            return $"<span class='signature-text'>{signatureData}</span>";
        }
    }
} 