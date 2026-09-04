namespace ZaggyCode.Core.Theming.Interfaces;

public interface IThemeCopier
{
    Task<string> CopyThemeAsync(ThemeMetadata theme, CancellationToken token = default);
}
