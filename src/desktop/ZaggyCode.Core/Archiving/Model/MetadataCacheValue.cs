namespace ZaggyCode.Core.Archiving.Model;

public sealed record MetadataCacheValue<T>(DateTime EditTime, T ArchiveMetadata) where T : ArchiveMetadata;