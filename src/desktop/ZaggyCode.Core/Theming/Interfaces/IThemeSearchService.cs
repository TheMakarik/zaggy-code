namespace ZaggyCode.Core.Theming.Interfaces;

public interface IThemeSearchService
{
    IReadOnlyList<ThemeMetadata> Search(IReadOnlyList<ThemeMetadata> themes, string? query);
}
