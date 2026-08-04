namespace ZaggyCode.Modules.Archiving;

public sealed class TarBZip2ArchiveReader(
    ILogger<TarBZip2ArchiveReader> logger,
    IMetadataParser metadataParser,
    IOptions<TempOptions> tempOptions) : IArchiveReader
{
    public async IAsyncEnumerable<T> EnumerateMetadata<T>(
        IReadOnlyCollection<string> archiveDirectories,
        string extension,
        bool recursive) where T : ArchiveMetadata
    {
        Debug.Assert(extension.StartsWith('.'), "extensions must starts with '.'");
        Debug.Assert(archiveDirectories.All(Directory.Exists), "All directories must exists");

        logger.LogDebug("Start enumerating archives metadata in directories: {dirs}",
            string.Join(", ", archiveDirectories));

        var files = ReadArchiveFiles(extension, recursive, archiveDirectories);
        var metadataFilesCount = 0;
        await foreach (var file in files.ToAsyncEnumerable())
        {
            await using var archive = await OpenArchive(file);
            if (archive is null)
                continue;

            var tarEntries = archive.EntriesAsync
                .Where(entry => entry is { Key: not null, IsDirectory: false })
                .Select(entry => entry.Key!);

            var metadata = await ParseMetadataAsync<T>(tarEntries, archive, file);

            if (metadata is null)
                continue;

            metadataFilesCount++;
            yield return metadata;
        }

        logger.LogInformation("Enumerated {c} metadata files", metadataFilesCount);
    }

    private async Task<T?> ParseMetadataAsync<T>(IAsyncEnumerable<string> tarEntries,
        IWritableAsyncArchive<TarWriterOptions> archive, string file) where T : ArchiveMetadata
    {
        try
        {
            var metadataFile = await metadataParser.SelectMetadataFileAsync(tarEntries);
            var metaDataEntry = await archive.EntriesAsync.FirstAsync(entry => entry.Key == metadataFile);
            await using var stream = await metaDataEntry.OpenEntryStreamAsync();
            return metadataParser.Parse<T>(stream);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to open archive from {file}", file);
            return null;
        }
    }

    private IEnumerable<string> ReadArchiveFiles(string extension, bool recursive,
        IReadOnlyCollection<string> archiveDirectories)
    {
        foreach (var directory in archiveDirectories)
        {
            logger.LogDebug("Searching archives in directory: {directory}", directory);
            var files = Directory.EnumerateFiles(directory, $"*{extension}",
                recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            foreach (var file in files)
                yield return file;
        }
    }

    private async Task<IWritableAsyncArchive<TarWriterOptions>?> OpenArchive(string file)
    {
        var stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read);

        if (!await BZip2Stream.IsBZip2Async(stream))
        {
            logger.LogError("File {file} is not BZip2 archive", file);
            await stream.DisposeAsync();
            return null;
        }

        stream.Seek(0, SeekOrigin.Begin);

        var bzip2Stream = await BZip2Stream.CreateAsync(
            stream,
            CompressionMode.Decompress,
            decompressConcatenated: false);

        if (!await TarArchive.IsTarFileAsync(bzip2Stream))
        {
            logger.LogError("File {file} is not .tar.bz2 archive", file);
            await bzip2Stream.DisposeAsync();
            return null;
        }

        var tarArchive = await TarArchive.OpenAsyncArchive(bzip2Stream);
        return tarArchive;
    }

    public async Task<DirectoryInfo> ExtractAllToTempAsync(string archivePath)
    {
        Debug.Assert(File.Exists(archivePath));

        var tempDirectory = CreateTempDirectory();
        var archive = await OpenArchive(archivePath);

        if (archive is null)
            throw new InvalidOperationException($"Failed to open archive '{archivePath}'");

        await using (archive)
        {
            await archive.WriteToDirectoryAsync(
                tempDirectory.FullName,
                new ExtractionOptions { ExtractFullPath = true, Overwrite = true });
        }

        return tempDirectory;
    }

    private DirectoryInfo CreateTempDirectory()
    {
        var tempPath = Path.Join(tempOptions.Value.TempDirectoryPath, Guid.NewGuid().ToString("N"));
        return Directory.CreateDirectory(tempPath);
    }
}
