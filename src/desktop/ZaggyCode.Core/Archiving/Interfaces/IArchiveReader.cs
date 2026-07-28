using Archive = ZaggyCode.Core.Archiving.Model.Archive;

namespace ZaggyCode.Core.Archiving.Interfaces;

public interface IArchiveReader
{
    public Archive Open(string path);
    public T ReadMetadata<T>(Archive archive) where T : ArchiveMetadata;
    public FileStream ReadFile(string name);
    public Task<DirectoryInfo> ExtractToTempAsync();
}
