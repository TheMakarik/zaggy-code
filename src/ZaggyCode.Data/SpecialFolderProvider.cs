using ZaggyCode.Data.Interfaces;
using ZaggyCode.Shared.Attributes;

namespace ZaggyCode.Data;

[SingletonService]
//Нужно чтоб мокнуть получения папки в тестах чтоб не переписывать их и не ломать свою либу
public class SpecialFolderProvider : ISpecialFolderProvider
{
    public string GetFolder(Environment.SpecialFolder folder, string path)
    {
        return Path.Join(Environment.GetFolderPath(folder), path);
    }
}