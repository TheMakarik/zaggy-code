namespace ZaggyCode.Tests.Infrastructure;

public sealed class TestFileSystem : IDisposable
{
    private readonly DirectoryInfo _root;

    public string RootPath => _root.FullName;

    public TestFileSystem()
    {
        var path = Path.Join(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        _root = Directory.CreateDirectory(path);
    }

    public string CreateFile(string extension)
    {
        var name = Guid.NewGuid().ToString("N") + extension;
        var path = Path.Join(_root.FullName, name);
        File.WriteAllText(path, string.Empty);
        return path;
    }

    public string CreateDirectory()
    {
        var name = Guid.NewGuid().ToString("N");
        var path = Path.Join(_root.FullName, name);
        return Directory.CreateDirectory(path).FullName;
    }

    public void Dispose()
    {
        try
        {
            _root.Delete(recursive: true);
        }
        catch
        {
            // Best effort cleanup.
        }
    }
}
