namespace ZaggyCode.Core.Theming.Interfaces;

public interface IThemeDeleter
{
    Task DeleteThemeAsync(ThemeMetadata theme, CancellationToken token = default);
}
