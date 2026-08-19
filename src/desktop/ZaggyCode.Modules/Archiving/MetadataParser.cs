namespace ZaggyCode.Modules.Archiving;

//#:NO_AI
public sealed class MetadataParser(IServiceProvider provider, IOptions<MetadataOptions> options) : IMetadataParser
{
    public T Parse<T>(Stream stream) where T : ArchiveMetadata
    {
        var serializer = provider.GetRequiredService<XmlSerializer<T>>();
        return serializer.Deserialize(stream) ?? throw new InvalidOperationException("XML File is corrupted");
    }

    public async Task<string> SelectMetadataFileAsync(IAsyncEnumerable<string> files)
    {
        return await files.FirstAsync(file => file == options.Value.MetadataFile);
    }
}