namespace ZaggyCode.Modules.Theming;

public sealed class ThemeDeleter(ILogger<ThemeDeleter> logger) : IThemeDeleter
{
    public async Task DeleteThemeAsync(ThemeMetadata theme, CancellationToken token = default)
    {
        if (theme.IsSystemTheme)
            throw new InvalidOperationException($"Cannot delete system theme '{theme.Name}'");

        if (string.IsNullOrWhiteSpace(theme.Path) || !File.Exists(theme.Path))
            throw new FileNotFoundException($"Theme archive '{theme.Name}' was not found", theme.Path);

        await Task.Run(() => File.Delete(theme.Path), token);
        logger.LogInformation("Deleted theme '{name}' ({path})", theme.Name, theme.Path);
    }
}
