namespace ZaggyCode.Modules.Theming;

public sealed class ThemeCopier(ILogger<ThemeCopier> logger, IOptions<ThemeOptions> themeOptions) : IThemeCopier
{
    public async Task<string> CopyThemeAsync(ThemeMetadata theme, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(theme.Path) || !File.Exists(theme.Path))
            throw new FileNotFoundException($"Theme archive '{theme.Name}' was not found", theme.Path);

        var copyName = BuildCopyName(theme.Name);
        var targetPath = Path.Join(themeOptions.Value.ExternThemesFolder, $"{copyName}{themeOptions.Value.ThemeExtensions}");
        Directory.CreateDirectory(themeOptions.Value.ExternThemesFolder);

        await Task.Run(() => File.Copy(theme.Path, targetPath, overwrite: true), token);
        logger.LogInformation("Copied theme '{name}' to {path}", theme.Name, targetPath);
        return targetPath;
    }

    private string BuildCopyName(string name)
    {
        var candidate = $"{name} - копия";
        var index = 1;
        while (ThemeExists(candidate))
        {
            candidate = $"{name} - копия ({index})";
            index++;
        }

        return candidate;
    }

    private bool ThemeExists(string copyName)
        => File.Exists(Path.Join(themeOptions.Value.ExternThemesFolder, $"{copyName}{themeOptions.Value.ThemeExtensions}"));
}
