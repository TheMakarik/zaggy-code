namespace ZaggyCode.Modules.Archiving;

public sealed class MetadataCache(ILogger<IMetadataCache> logger) : IMetadataCache
{
    public ConcurrentDictionary<string, MetadataCacheValue> Caches { get; } = new();

    public MetadataCacheValue<T>? GetOrNull<T>(string archivePath) where T : ArchiveMetadata
    {
        
    }

    public void Add<T>(string archivePath, MetadataCacheValue<T> metadataCacheValue) where T : ArchiveMetadata
    {
        throw new NotImplementedException();
    }
}