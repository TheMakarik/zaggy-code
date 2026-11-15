namespace ZaggyCode.Core.Archiving.Interfaces;

public interface IMetadataParser
{
    public Task<T> ParseAsync<T>(Func<ValueTask<Stream>> getStreamLazyFunction, string filePath) where T : ArchiveMetadata;
    public Task<string> SelectMetadataFileAsync(IAsyncEnumerable<string> files);
}
