using SharpCompress.Common;
using SharpCompress.Writers;
using ZaggyCode.Modules.HostedServices.Options;

namespace ZaggyCode.Modules.HostedServices;

public sealed class LoggingCompressHostedService(
    ILogger<LoggingCompressHostedService> logger,
    IOptions<LoggingCompressOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        CompressOldLogs();

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            CompressOldLogs();
        }
    }

    private void CompressOldLogs()
    {
        var logsDirectory = options.Value.LogsDirectoryPath;

        if (!Directory.Exists(logsDirectory))
            return;

        var cutoff = DateTime.Now.AddDays(-options.Value.RetentionDays);
        var files = Directory.GetFiles(logsDirectory)
            .Where(file => File.GetLastWriteTime(file) < cutoff)
            .ToArray();

        if (files.Length == 0)
            return;

        var archivesDirectory = options.Value.ArchivesDirectoryPath;
        Directory.CreateDirectory(archivesDirectory);

        var archiveName = $"logs-{DateTime.Now:yyyyMMdd-HHmmss}.tar.bz2";
        var archivePath = Path.Join(archivesDirectory, archiveName);

        try
        {
            using var stream = File.Create(archivePath);
            using var writer = WriterFactory.OpenWriter(stream, ArchiveType.Tar, new WriterOptions(CompressionType.BZip2));

            foreach (var file in files)
            {
                writer.Write(Path.GetFileName(file), file);
                File.Delete(file);
            }

            logger.LogInformation("Compressed {Count} log file(s) older than {RetentionDays} days into {ArchivePath}",
                files.Length, options.Value.RetentionDays, archivePath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to compress old log files");
        }
    }
}
