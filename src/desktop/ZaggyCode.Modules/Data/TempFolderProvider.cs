namespace ZaggyCode.Modules.Data;

public sealed class TempFolderProvider(IOptions<TempOptions> tempOptions) : ITempFolderProvider
{
    public string GetTempPath()
        => GetOrCreate(tempOptions.Value.TempDirectoryPath);

    public string GetToCompressPath()
        => GetOrCreate(tempOptions.Value.TempToCompress);

    public string GetFromCompressPath()
        => GetOrCreate(tempOptions.Value.TempFromCompress);

    private static string GetOrCreate(string path)
    {
        Directory.CreateDirectory(path);
        return path;
    }
}
