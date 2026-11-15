namespace ZaggyCode.Core.Archiving.Model;

public sealed record MetadataCacheValue(DateTime LastWriteTimeUtc, ArchiveMetadata ArchiveMetadata);
