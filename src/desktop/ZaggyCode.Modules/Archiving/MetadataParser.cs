namespace ZaggyCode.Modules.Archiving;

public sealed class MetadataParser(IOptions<MetadataOptions> options) : IMetadataParser
{
    public T Parse<T>(Stream stream) where T : ArchiveMetadata
    {
        var metadata = JsonSerializer.Deserialize<T>(stream)
                       ?? throw new InvalidOperationException("Failed to deserialize theme metadata");

        return metadata;
    }

    public async Task<string> SelectMetadataFileAsync(IAsyncEnumerable<string> files)
    {
        var metadataFile = await files.FirstOrDefaultAsync(file =>
            file == options.Value.MetadataFile ||
            file.EndsWith($"/{options.Value.MetadataFile}", StringComparison.Ordinal));

        return metadataFile ?? throw new FileNotFoundException($"Archive does not contain {options.Value.MetadataFile}");
    }
}