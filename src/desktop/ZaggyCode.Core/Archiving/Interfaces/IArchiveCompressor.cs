namespace ZaggyCode.Core.Archiving.Interfaces;

public interface IArchiveCompressor : IDisposable, IAsyncDisposable
{
    public Task CompressAsync(string pathToArchive, string folderToCompress);
}

