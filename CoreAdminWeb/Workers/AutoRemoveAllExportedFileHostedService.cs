namespace CoreAdminWeb.Workers
{
    public sealed class AutoRemoveAllExportedFileHostedService : IHostedService, IDisposable
    {
        private readonly ILogger<AutoRemoveAllExportedFileHostedService> _logger;
        private CancellationTokenSource? _cts;
        private Task? _backgroundTask;
        private readonly TimeSpan _retention = TimeSpan.FromMinutes(30);
        private readonly TimeSpan _scanInterval = TimeSpan.FromMinutes(5);
        private readonly string _baseFolder;

        public AutoRemoveAllExportedFileHostedService(ILogger<AutoRemoveAllExportedFileHostedService> logger)
        {
            _logger = logger;
            _baseFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "exports", "kham_suc_khoe");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting AutoRemoveAllExportedFileHostedService. BaseFolder={BaseFolder}", _baseFolder);

            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _backgroundTask = Task.Run(() => DoWorkAsync(_cts.Token));

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping AutoRemoveAllExportedFileHostedService...");

            if (_cts == null)
                return;

            try
            {
                _cts.Cancel();

                if (_backgroundTask != null)
                {
                    await Task.WhenAny(_backgroundTask, Task.Delay(TimeSpan.FromSeconds(10), cancellationToken));
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error while stopping AutoRemoveAllExportedFileHostedService");
            }
        }

        private async Task DoWorkAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        CleanOldFilesAndDirectories();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error while cleaning exported files");
                    }

                    await Task.Delay(_scanInterval, token);
                }
            }
            catch (OperationCanceledException)
            {
                // expected on shutdown
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in AutoRemoveAllExportedFileHostedService background task");
            }
        }

        private void CleanOldFilesAndDirectories()
        {
            if (!Directory.Exists(_baseFolder))
            {
                // nothing to do
                return;
            }

            var now = DateTime.UtcNow;
            var threshold = now - _retention;

            try
            {
                var topLevelDirs = Directory.EnumerateDirectories(_baseFolder, "*", SearchOption.TopDirectoryOnly);
                foreach (var dir in topLevelDirs)
                {
                    try
                    {
                        var di = new DirectoryInfo(dir);
                        var createdUtc = di.CreationTimeUtc;

                        if (createdUtc < threshold) di.Delete(true);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "Failed to delete directory: {Path}", dir);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error enumerating top-level directories in {BaseFolder}", _baseFolder);
            }

            try
            {
                foreach (var file in Directory.EnumerateFiles(_baseFolder, "*", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        var info = new FileInfo(file);
                        var createdUtc = info.CreationTimeUtc;
                        if (createdUtc < threshold) info.Delete();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "Failed to delete file: {Path}", file);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error enumerating files in {BaseFolder}", _baseFolder);
            }
        }

        public void Dispose()
        {
            try
            {
                _cts?.Cancel();
            }
            catch { }

            _cts?.Dispose();
            _backgroundTask = null;
        }
    }
}
