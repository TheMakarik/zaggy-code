namespace ZaggyCode.Core.Theming.Interfaces;

public interface IThemeCreator
{
    Task CreateAsync(Theme theme, ThemeMetadata metadata, string outputDirectory);
}
