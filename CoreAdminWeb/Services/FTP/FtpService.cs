using CoreAdminWeb.Model.Configuration;
using System.Net;
using System.Text;

namespace CoreAdminWeb.Services.FTP
{
    /// <summary>
    /// Interface for FTP operations
    /// </summary>
    public interface IFtpService
    {
        Task<string> DownloadFileAsync(string fileName);
        Task<byte[]> DownloadFileAsBytesAsync(string fileName);
        Task<List<string>> ListFilesAsync(string directory = "");
        Task<bool> FileExistsAsync(string fileName);
        Task<long> GetFileSizeAsync(string fileName);
    }

    /// <summary>
    /// Service for FTP operations
    /// </summary>
    public class FtpService : IFtpService
    {
        private readonly FTPConfig _ftpConfig;

        public FtpService(FTPConfig ftpConfig)
        {
            _ftpConfig = ftpConfig ?? throw new ArgumentNullException(nameof(ftpConfig));
        }

        /// <summary>
        /// Download file content as string from FTP server
        /// </summary>
        public async Task<string> DownloadFileAsync(string fileName)
        {
            try
            {
                var fileBytes = await DownloadFileAsBytesAsync(fileName);
                return Encoding.UTF8.GetString(fileBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading file as string: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Download file content as byte array from FTP server
        /// </summary>
        public async Task<byte[]> DownloadFileAsBytesAsync(string fileName)
        {
            // First check if file exists
            Console.WriteLine($"Checking if file exists before download: {fileName}");
            var fileExists = await FileExistsAsync(fileName);
            
            if (!fileExists)
            {
                throw new FileNotFoundException($"File not found on FTP server: {fileName}");
            }

            // Get file size for verification
            var fileSize = await GetFileSizeAsync(fileName);
            Console.WriteLine($"File size reported: {fileSize} bytes");

            // Try multiple download methods
            var methods = new (string Name, Func<Task<byte[]>> Method)[]
            {
                ("Standard Download", () => DownloadWithStandardMethod(fileName)),
                ("Binary Download", () => DownloadWithBinaryMethod(fileName)),
                ("Alternative Download", () => DownloadWithAlternativeMethod(fileName))
            };

            Exception lastException = null;
            
            foreach (var methodInfo in methods)
            {
                try
                {
                    Console.WriteLine($"Trying {methodInfo.Name} for file: {fileName}");
                    var result = await methodInfo.Method();
                    if (result != null && result.Length > 0)
                    {
                        Console.WriteLine($"{methodInfo.Name} successful: {result.Length} bytes (expected: {fileSize})");
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{methodInfo.Name} failed: {ex.Message}");
                    lastException = ex;
                }
            }
            
            throw lastException ?? new Exception("All download methods failed");
        }

        private async Task<byte[]> DownloadWithStandardMethod(string fileName)
        {
            string ftpUrl;
            if (fileName.StartsWith("ftp://"))
            {
                ftpUrl = fileName;
            }
            else
            {
                ftpUrl = $"ftp://{_ftpConfig.Host}:{_ftpConfig.Port}/{_ftpConfig.Folder}/{fileName}";
            }
            
            Console.WriteLine($"Standard FTP Download URL: {ftpUrl}");
            
            var request = (FtpWebRequest)WebRequest.Create(ftpUrl);
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.Credentials = new NetworkCredential(_ftpConfig.Username, _ftpConfig.Password);
            request.UsePassive = _ftpConfig.UsePassive;
            request.UseBinary = true;
            request.KeepAlive = false;

            using var response = (FtpWebResponse)await request.GetResponseAsync();
            using var responseStream = response.GetResponseStream();
            using var memoryStream = new MemoryStream();
            
            await responseStream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }

        private async Task<byte[]> DownloadWithBinaryMethod(string fileName)
        {
            string ftpUrl;
            if (fileName.StartsWith("ftp://"))
            {
                ftpUrl = fileName;
            }
            else
            {
                ftpUrl = $"ftp://{_ftpConfig.Host}:{_ftpConfig.Port}/{_ftpConfig.Folder}/{fileName}";
            }
            
            Console.WriteLine($"Binary FTP Download URL: {ftpUrl}");
            
            var request = (FtpWebRequest)WebRequest.Create(ftpUrl);
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.Credentials = new NetworkCredential(_ftpConfig.Username, _ftpConfig.Password);
            request.UsePassive = !_ftpConfig.UsePassive; // Try opposite passive mode
            request.UseBinary = true;
            request.KeepAlive = true; // Try keeping connection alive
            request.Timeout = 30000; // 30 second timeout

            using var response = (FtpWebResponse)await request.GetResponseAsync();
            using var responseStream = response.GetResponseStream();
            using var memoryStream = new MemoryStream();
            
            await responseStream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }

        private async Task<byte[]> DownloadWithAlternativeMethod(string fileName)
        {
            // Try without leading slash
            var cleanFileName = fileName.TrimStart('/');
            string ftpUrl;
            
            if (cleanFileName.StartsWith("ftp://"))
            {
                ftpUrl = cleanFileName;
            }
            else
            {
                ftpUrl = $"ftp://{_ftpConfig.Host}:{_ftpConfig.Port}/{_ftpConfig.Folder}/{cleanFileName}";
            }
            
            Console.WriteLine($"Alternative FTP Download URL: {ftpUrl}");
            
            var request = (FtpWebRequest)WebRequest.Create(ftpUrl);
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.Credentials = new NetworkCredential(_ftpConfig.Username, _ftpConfig.Password);
            request.UsePassive = _ftpConfig.UsePassive;
            request.UseBinary = false; // Try ASCII mode
            request.KeepAlive = false;

            using var response = (FtpWebResponse)await request.GetResponseAsync();
            using var responseStream = response.GetResponseStream();
            using var memoryStream = new MemoryStream();
            
            await responseStream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }

        /// <summary>
        /// Check if file exists on FTP server
        /// </summary>
        public async Task<bool> FileExistsAsync(string fileName)
        {
            try
            {
                string ftpUrl;
                if (fileName.StartsWith("ftp://"))
                {
                    ftpUrl = fileName;
                }
                else
                {
                    ftpUrl = $"ftp://{_ftpConfig.Host}:{_ftpConfig.Port}/{_ftpConfig.Folder}/{fileName}";
                }
                
                Console.WriteLine($"Checking FTP file exists: {ftpUrl}");
                
                var request = (FtpWebRequest)WebRequest.Create(ftpUrl);
                request.Method = WebRequestMethods.Ftp.GetFileSize;
                request.Credentials = new NetworkCredential(_ftpConfig.Username, _ftpConfig.Password);
                request.UsePassive = _ftpConfig.UsePassive;
                
                using var response = (FtpWebResponse)await request.GetResponseAsync();
                Console.WriteLine($"File exists: {ftpUrl}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"File does not exist {fileName}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get file size from FTP server
        /// </summary>
        public async Task<long> GetFileSizeAsync(string fileName)
        {
            try
            {
                string ftpUrl;
                if (fileName.StartsWith("ftp://"))
                {
                    ftpUrl = fileName;
                }
                else
                {
                    ftpUrl = $"ftp://{_ftpConfig.Host}:{_ftpConfig.Port}/{_ftpConfig.Folder}/{fileName}";
                }
                
                Console.WriteLine($"Getting FTP file size: {ftpUrl}");
                
                var request = (FtpWebRequest)WebRequest.Create(ftpUrl);
                request.Method = WebRequestMethods.Ftp.GetFileSize;
                request.Credentials = new NetworkCredential(_ftpConfig.Username, _ftpConfig.Password);
                request.UsePassive = _ftpConfig.UsePassive;
                
                using var response = (FtpWebResponse)await request.GetResponseAsync();
                Console.WriteLine($"File size: {ftpUrl} = {response.ContentLength} bytes");
                return response.ContentLength;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting file size {fileName}: {ex.Message}");
                return -1;
            }
        }

      
        /// <summary>
        /// List files in FTP directory
        /// </summary>
        public async Task<List<string>> ListFilesAsync(string directory = "")
        {
            try
            {
                string ftpUrl = $"ftp://{_ftpConfig.Host}:{_ftpConfig.Port}/{_ftpConfig.Folder}/{directory}";
                var request = (FtpWebRequest)WebRequest.Create(ftpUrl);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(_ftpConfig.Username, _ftpConfig.Password);
                request.UsePassive = _ftpConfig.UsePassive;
                request.KeepAlive = false;

                using var response = (FtpWebResponse)await request.GetResponseAsync();
                using var responseStream = response.GetResponseStream();
                using var reader = new StreamReader(responseStream);
                
                var files = new List<string>();
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    files.Add(line);
                }
                
                return files;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error listing files in directory {directory}: {ex.Message}");
                return new List<string>();
            }
        }
    }

    /// <summary>
    /// Legacy FtpClient class for backward compatibility
    /// </summary>
    public class FtpClient
    {
        private readonly IFtpService _ftpService;

        public FtpClient(string host, int port, string username, string password)
        {
            var config = new FTPConfig
            {
                Host = host,
                Port = port,
                Username = username,
                Password = password,
                UsePassive = true
            };
            
            _ftpService = new FtpService(config);
        }

        public async Task<string> DownloadFileAsync(string fileName)
        {
            return await _ftpService.DownloadFileAsync(fileName);
        }
    }
} 