using  CoreAdminWeb.Model;
namespace CoreAdminWeb.Helpers
{
    public class FileHelpers
    {
        public static string ConvertSize(long size)
        {
            string[] units = { "B", "KB", "MB", "GB", "TB" };
            double convertedSize = size;
            int unitIndex = 0;
            
            while(convertedSize >= 1024 && unitIndex < units.Length - 1)
            {
                convertedSize /= 1024;
                unitIndex++;
            }

            return $"{convertedSize:F2} {units[unitIndex]}";
        }


        public static string MinifyFileName(string fileName, int maxLength =15)
        {
            if(fileName.Length > maxLength)
            {
                return fileName.Substring(0, maxLength) + "...";
            }
            return fileName;
        }

        public static string GetFileExtension(string fileName)
        {
            return Path.GetExtension(fileName).ToLower();
        }

        public static (string, string) GetFileType(string fileName)
        {
            string extension = GetFileExtension(fileName);
            switch(extension)
            {
                case ".pdf":
                    return ("PDF", "picture_as_pdf");
                case ".doc":
                case ".docx":
                case ".dotx":
                case ".dotm":
                case ".docm":
                case ".dot":
                    return ("Word", "description");
                case ".xls":
                case ".xlsx":
                case ".xlsm":
                case ".xlsb":
                case ".xltx":
                case ".xltm":
                case ".xlt":
                case ".xlam":
                    return ("Excel", "table");
                case ".ppt":
                case ".pptx":
                    return ("PowerPoint", "wallpaper_slideshow");
                case ".txt":
                case ".csv":
                case ".tsv":
                case ".rtf":
                case ".odt":
                case ".ods":
                case ".odp":
                    return ("Text", "text_snippet");
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".svg":
                case ".webp":
                case ".gif":
                case ".bmp":
                case ".tiff":
                case ".ico":
                case ".heic":
                case ".heif":
                case ".hevc":
                    return ("Image", string.Empty);
                case ".mp4":
                case ".mov":
                case ".avi":
                case ".mkv":
                case ".wmv":
                case ".flv":
                case ".mpeg":
                case ".mp3":
                case ".ogg":
                case ".wav":
                case ".m4a":
                case ".aac":
                case ".flac":
                case ".m4v":
                case ".webm":
                    return ("Audio", "music_note");
                case ".zip":
                case ".rar":
                case ".7z":
                    return ("Archive", "file_present");

                case ".html":
                case ".py":
                case ".cs":
                case ".js":
                case ".css":
                case ".php":
                case ".sql":
                case ".json":
                case ".xml":
                    return ("Code", "code");
                default:
                    return ("File", "file_present");  
            }
        }
    }

}