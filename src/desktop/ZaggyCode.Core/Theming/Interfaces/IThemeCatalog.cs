namespace ZaggyCode.Core.Theming.Interfaces;

public interface IThemeCatalog
{
    Task<IReadOnlyList<ThemeMetadata>> GetAvailableThemesAsync(CancellationToken token = default);

    Task<Theme?> LoadThemeAsync(string name, CancellationToken token = default);
}
