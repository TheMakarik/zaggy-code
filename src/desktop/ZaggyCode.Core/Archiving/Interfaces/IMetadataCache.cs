using System.Diagnostics.CodeAnalysis;

namespace ZaggyCode.Core.Archiving.Interfaces;

public interface IMetadataCache
{
    public bool TryGetValue<T>(
        string archivePath,
        DateTime lastWriteTimeUtc,
        [NotNullWhen(true)] out T? metadata) where T : ArchiveMetadata;

    public void Add<T>(string archivePath, DateTime lastWriteTimeUtc, T metadata) where T : ArchiveMetadata;
}
