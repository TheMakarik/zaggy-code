namespace ZaggyCode.Core.Data.Interfaces;

public interface IThemeReader
{
    public IAsyncEnumerable<ThemeMetadata> EnumerateMetadata();
    public void SetTheme(string themeName);
}
