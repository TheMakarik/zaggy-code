namespace ZaggyCode.Modules.Archiving;

public sealed class TarBZip2ArchiveCompressor(
    ILogger<TarBZip2ArchiveCompressor> logger,
    IOptions<TempOptions> tempOptions) : IArchiveCompressor, IDisposable, IAsyncDisposable
{
    public async Task CompressAsync(string pathToArchive, string folderToCompress)
    {
        Debug.Assert(Directory.Exists(folderToCompress), "Folder to compress must exist");

        var toCompressRoot = Path.GetFullPath(tempOptions.Value.TempToCompress);
        var fullFolderPath = Path.GetFullPath(folderToCompress);
        if (!fullFolderPath.StartsWith(toCompressRoot, StringComparison.Ordinal))
            throw new ArgumentException(
                $"Folder '{folderToCompress}' must be placed inside to-compress directory '{toCompressRoot}'",
                nameof(folderToCompress));

        logger.LogInformation("Compressing {folder} into {archive}", fullFolderPath, pathToArchive);

        await using var archiveStream = File.Open(pathToArchive, FileMode.Create, FileAccess.Write);
        using var writer = WriterFactory.OpenWriter(
            archiveStream,
            ArchiveType.Tar,
            new TarWriterOptions(CompressionType.BZip2, true));

        foreach (var file in Directory.EnumerateFiles(fullFolderPath, "*", SearchOption.AllDirectories))
        {
            var entryName = Path.GetRelativePath(fullFolderPath, file).Replace('\\', '/');
            await using var fileStream = File.OpenRead(file);
            writer.Write(entryName, fileStream, modificationTime: null);
        }

        logger.LogInformation("Compressed {folder} into {archive}", fullFolderPath, pathToArchive);
    }

    public void Dispose() { }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
