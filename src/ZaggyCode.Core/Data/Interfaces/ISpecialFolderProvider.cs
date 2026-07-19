namespace ZaggyCode.Core.Data.Interfaces;

public interface ISpecialFolderProvider
{
    public string GetFolder(Environment.SpecialFolder folder, string path);
}