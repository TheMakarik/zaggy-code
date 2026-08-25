
namespace ZaggyCode.Core.Archiving.Interfaces;

public interface IArchiveReader
{
   public IAsyncEnumerable<T> EnumerateMetadata<T>(IReadOnlyCollection<string> archiveDirectories, string extension, bool recursive) where T : ArchiveMetadata;
   public Task<DirectoryInfo> ExtractAllToTempAsync(string archivePath, IProgress<int> oneHundredPerCentBasedProgress);
   public Task<T?> ReadMetadataAsync<T>(string archivePath) where T : ArchiveMetadata;
}
