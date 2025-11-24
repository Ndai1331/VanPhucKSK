using System.Diagnostics;

namespace CoreAdminWeb.Workers
{
    public class LibreOfficeUnoListenerHostedService : IHostedService
    {
        private readonly ILogger<LibreOfficeUnoListenerHostedService> _logger;
        private readonly IConfiguration _config;
        private Process? _process;

        public LibreOfficeUnoListenerHostedService(
            ILogger<LibreOfficeUnoListenerHostedService> logger,
            IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var mode = _config["LibreOffice:Mode"] ?? "Uno";
            if (!string.Equals(mode, "Uno", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation(
                    "LibreOffice UNO listener not started because LibreOffice:Mode={Mode}",
                    mode);
                return Task.CompletedTask;
            }

            var sofficePath = _config["LibreOffice:SofficePath"] ?? "/usr/bin/soffice";
            var host = _config["LibreOffice:UnoHost"] ?? "127.0.0.1";
            var port = _config.GetValue("LibreOffice:UnoPort", 2002);

            var psi = new ProcessStartInfo
            {
                FileName = sofficePath,
                ArgumentList =
                {
                    "--headless",
                    "--nologo",
                    "--nodefault",
                    "--norestore",
                    "--nofirststartwizard",
                    $"--accept=socket,host={host},port={port};urp;"
                },
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            _logger.LogInformation(
                "Starting LibreOffice UNO listener at {Host}:{Port}",
                host, port);

            try
            {
                _process = Process.Start(psi);

                if (_process == null || _process.HasExited)
                {
                    _logger.LogError("Failed to start LibreOffice UNO listener.");
                }
                else
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var stdout = await _process.StandardOutput.ReadToEndAsync(cancellationToken);
                            var stderr = await _process.StandardError.ReadToEndAsync(cancellationToken);

                            if (!string.IsNullOrWhiteSpace(stdout))
                                _logger.LogInformation("[UNO-LISTENER-OUT] {StdOut}", stdout);

                            if (!string.IsNullOrWhiteSpace(stderr))
                                _logger.LogWarning("[UNO-LISTENER-ERR] {StdErr}", stderr);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error reading LibreOffice UNO listener output");
                        }
                    }, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while starting LibreOffice UNO listener.");
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (_process != null && !_process.HasExited)
            {
                _logger.LogInformation("Stopping LibreOffice UNO listener (PID={Pid})", _process.Id);
                try
                {
                    _process.Kill(entireProcessTree: true);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error when stopping LibreOffice UNO listener");
                }
            }

            return Task.CompletedTask;
        }
    }
}
