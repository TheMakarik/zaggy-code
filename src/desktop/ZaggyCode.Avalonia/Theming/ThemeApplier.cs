namespace ZaggyCode.Avalonia.Theming;

public sealed class ThemeApplier(
    ILogger<ThemeApplier> logger,
    IThemeCatalog themeCatalog,
    IThemeSetterProxy themeSetterProxy,
    IObservableStorage<UserData> userStorage) : IThemeApplier
{
    private const string FallbackThemeName = "Primus";
    private const string ColorSuffix = "Color";

    // Имя ресурса выводится из свойства темы: BackgroundColor → Background (+ Color/Brush в прокси).
    private static readonly PropertyInfo[] ThemeColorProperties =
        typeof(Theme).GetProperties()
            .Where(property => property.PropertyType == typeof(string))
            .ToArray();

    public async Task ApplySavedThemeAsync()
        => await ApplyThemeAsync(userStorage.Current.CurrentTheme);

    public async Task ApplyThemeAsync(string name)
    {
        var theme = await LoadThemeOrFallbackAsync(name);
        if (theme is null)
            return;

        foreach (var property in ThemeColorProperties)
        {
            var hex = (string?)property.GetValue(theme);
            if (hex is null)
                continue;

            var resourceName = property.Name.EndsWith(ColorSuffix, StringComparison.Ordinal)
                ? property.Name[..^ColorSuffix.Length]
                : property.Name;
            themeSetterProxy.SetColor(resourceName, hex);
        }

        logger.LogInformation("Applied theme {name}", name);
    }

    private async Task<Theme?> LoadThemeOrFallbackAsync(string name)
    {
        var theme = await themeCatalog.LoadThemeAsync(name);
        if (theme is not null)
            return theme;

        logger.LogWarning("Theme {name} was not found, falling back to {fallback}", name, FallbackThemeName);

        userStorage.Current.CurrentTheme = FallbackThemeName;
        await ShowThemeNotFoundDialogAsync(name);

        return await themeCatalog.LoadThemeAsync(FallbackThemeName);
    }

    private async Task ShowThemeNotFoundDialogAsync(string missingThemeName)
    {
        try
        {
            var owner = GetOwnerWindow();
            if (owner is null)
                return;

            var dialog = new ThemeNotFoundWindow(missingThemeName)
            {
                WindowDecorations = userStorage.Current.UseSystemTitleBar
                    ? WindowDecorations.Full
                    : WindowDecorations.BorderOnly
            };
            await dialog.ShowDialog(owner);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to show theme-not-found dialog");
        }
    }

    private static Window? GetOwnerWindow()
        => Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;
}
