using CoreAdminWeb.Helpers;
using System.Diagnostics;
using System.Text;

namespace CoreAdminWeb.Services.DocxToPdfConverter
{
    public sealed class LibreOfficeDocxToPdfConverter : IDocxToPdfConverter
    {
        private readonly ILogger<LibreOfficeDocxToPdfConverter> _logger;
        private readonly string _sofficePath;

        // Giới hạn tối đa số process LibreOffice chạy song song
        private static readonly SemaphoreSlim _globalSemaphore = new(initialCount: 1, maxCount: 32);

        public LibreOfficeDocxToPdfConverter(
            IConfiguration config,
            ILogger<LibreOfficeDocxToPdfConverter> logger)
        {
            _logger = logger;

            _sofficePath = LibreOfficePathResolver.GetSofficePath(config, logger);
        }

        public async Task<byte[]> ConvertFileAsync(string inputDocxPath, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(inputDocxPath))
                throw new ArgumentException("inputDocxPath is null or empty.", nameof(inputDocxPath));

            if (!File.Exists(inputDocxPath))
                throw new FileNotFoundException($"DOCX file not found: {inputDocxPath}", inputDocxPath);

            await _globalSemaphore.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                return await ConvertInternalAsync(inputDocxPath, ct).ConfigureAwait(false);
            }
            finally
            {
                _globalSemaphore.Release();
            }
        }

        private async Task<byte[]> ConvertInternalAsync(string inputDocxPath, CancellationToken ct)
        {
            var inputFullPath = Path.GetFullPath(inputDocxPath);

            // Tạo thư mục tạm riêng cho mỗi convert
            var tempRoot = Path.GetTempPath();
            var tempDir = Path.Combine(tempRoot, "lo-convert", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            _logger.LogDebug("Converting DOCX to PDF: {Input} -> TempDir: {TempDir}",
                inputFullPath, tempDir);

            try
            {
                // Arguments cho soffice: headless, convert sang pdf, outdir là tempDir
                var psi = new ProcessStartInfo
                {
                    FileName = _sofficePath,
                    Arguments = $"--headless --convert-to pdf --outdir \"{tempDir}\" \"{inputFullPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using var process = new Process { StartInfo = psi };
                var stdout = new StringBuilder();
                var stderr = new StringBuilder();

                process.OutputDataReceived += (_, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        stdout.AppendLine(e.Data);
                };
                process.ErrorDataReceived += (_, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        stderr.AppendLine(e.Data);
                };

                if (!process.Start())
                    throw new InvalidOperationException("Failed to start LibreOffice process.");

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // Chờ process xong, có hỗ trợ cancellation
                await WaitForExitAsync(process, ct).ConfigureAwait(false);

                if (process.ExitCode != 0)
                {
                    _logger.LogError(
                        "LibreOffice exited with code {ExitCode}. StdOut: {StdOut} StdErr: {StdErr}",
                        process.ExitCode, stdout.ToString(), stderr.ToString());

                    throw new InvalidOperationException(
                        $"LibreOffice convert failed (exit code {process.ExitCode}). Error: {stderr}");
                }

                // Tìm file pdf output trong tempDir
                var pdfFile = Directory.EnumerateFiles(tempDir, "*.pdf").FirstOrDefault();
                if (pdfFile == null)
                {
                    _logger.LogError(
                        "No PDF file generated for input: {Input}. StdOut: {StdOut}. StdErr: {StdErr}",
                        inputFullPath, stdout.ToString(), stderr.ToString());

                    throw new FileNotFoundException("PDF output not found after conversion.");
                }

                var bytes = await File.ReadAllBytesAsync(pdfFile, ct).ConfigureAwait(false);
                return bytes;
            }
            finally
            {
                try
                {
                    if (Directory.Exists(tempDir))
                        Directory.Delete(tempDir, recursive: true);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to cleanup LibreOffice temp directory: {TempDir}", tempDir);
                }
            }
        }

        private static Task WaitForExitAsync(Process process, CancellationToken cancellationToken)
        {
            if (process.HasExited)
                return Task.CompletedTask;

            var tcs = new TaskCompletionSource<object?>();

            process.Exited += (_, _) => tcs.TrySetResult(null);
            process.EnableRaisingEvents = true;

            if (cancellationToken != default)
            {
                cancellationToken.Register(() =>
                {
                    try
                    {
                        if (!process.HasExited)
                            process.Kill(entireProcessTree: true);
                    }
                    catch { /* ignore */ }

                    tcs.TrySetCanceled(cancellationToken);
                });
            }

            return tcs.Task;
        }
    }
}
