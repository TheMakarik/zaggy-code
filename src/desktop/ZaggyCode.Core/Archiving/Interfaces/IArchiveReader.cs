
namespace ZaggyCode.Core.Archiving.Interfaces;

public interface IArchiveReader
{
   public IAsyncEnumerable<T> EnumerateMetadataAsync<T>();
   public T ReadAsync<T>(string actualPath, string archivePath);
}
