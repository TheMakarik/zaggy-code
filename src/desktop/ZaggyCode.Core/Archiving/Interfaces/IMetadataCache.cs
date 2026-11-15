namespace ZaggyCode.Core.Archiving.Interfaces;

public interface IMetadataCache
{
    public MetadataCacheValue<T>? GetOrNull<T>(string archivePath) where T : ArchiveMetadata;
    public void Add<T>(string archivePath, MetadataCacheValue<T> metadataCacheValue) where T : ArchiveMetadata;
}