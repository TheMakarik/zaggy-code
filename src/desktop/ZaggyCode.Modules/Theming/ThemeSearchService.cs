namespace ZaggyCode.Modules.Theming;

public sealed class ThemeSearchService : IThemeSearchService
{
    public IReadOnlyList<ThemeMetadata> Search(IReadOnlyList<ThemeMetadata> themes, string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return themes;

        var comparison = StringComparison.OrdinalIgnoreCase;
        var nameMatches = themes.Where(theme => theme.Name.Contains(query, comparison)).ToList();
        var descriptionMatches = themes
            .Except(nameMatches)
            .Where(theme => (theme.Description ?? string.Empty).Contains(query, comparison))
            .ToList();

        return [.. nameMatches, .. descriptionMatches];
    }
}
