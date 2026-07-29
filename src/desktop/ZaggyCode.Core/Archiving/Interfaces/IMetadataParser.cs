namespace ZaggyCode.Core.Archiving.Interfaces;

public interface IMetadataParser
{
    public T Parse<T>(Stream stream) where T : ArchiveMetadata;
    public Task<string> SelectMetadataFileAsync(IAsyncEnumerable<string> files);
}