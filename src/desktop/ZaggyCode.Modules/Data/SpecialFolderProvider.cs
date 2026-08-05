namespace ZaggyCode.Modules.Data;

//Нужно чтоб мокнуть получения папки в тестах 
public class SpecialFolderProvider : ISpecialFolderProvider
{
    public string GetFolder(Environment.SpecialFolder folder, string path)
    {
        if (Path.IsPathRooted(path))
            return path;

        return Path.Join(Environment.GetFolderPath(folder), path);
    }
}
