using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace ZaggyCode.Modules.Archiving;

public sealed class MetadataCache(ILogger<IMetadataCache> logger) : IMetadataCache
{
    private static readonly StringComparer PathComparer =
        OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

    private readonly ConcurrentDictionary<string, MetadataCacheValue> _caches = new(PathComparer);

    public bool TryGetValue<T>(
        string archivePath,
        DateTime lastWriteTimeUtc,
        [NotNullWhen(true)] out T? metadata) where T : ArchiveMetadata
    {
        metadata = null;
        var key = NormalizePath(archivePath);

        if (!_caches.TryGetValue(key, out var value))
        {
            logger.LogDebug("Файл {path} не найден в кеше метаданных", archivePath);
            return false;
        }

        if (value.LastWriteTimeUtc != lastWriteTimeUtc)
        {
            _caches.TryRemove(key, out _);
            logger.LogDebug("Метаданные {path} устарели: файл был обновлён", archivePath);
            return false;
        }

        if (value.ArchiveMetadata is not T typed)
        {
            logger.LogError(
                "Метаданные архива {path} являются {actual}, а не {expected}",
                archivePath,
                value.ArchiveMetadata.GetType().FullName,
                typeof(T).FullName);
            return false;
        }

        metadata = typed;
        logger.LogDebug("Метаданные получены из кеша: {path}", archivePath);
        return true;
    }

    public void Add<T>(string archivePath, DateTime lastWriteTimeUtc, T metadata) where T : ArchiveMetadata
    {
        var key = NormalizePath(archivePath);
        _caches[key] = new MetadataCacheValue(lastWriteTimeUtc, metadata);

        logger.LogDebug(
            "Метаданные добавлены в кеш: {path}. Количество файлов в кеше: {count}",
            archivePath,
            _caches.Count);
    }

    private static string NormalizePath(string archivePath) => Path.GetFullPath(archivePath);
}
