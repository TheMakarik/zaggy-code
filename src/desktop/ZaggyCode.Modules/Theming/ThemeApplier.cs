namespace ZaggyCode.Modules.Theming;

public sealed class ThemeApplier(
    ILogger<ThemeApplier> logger,
    IThemeCatalog themeCatalog,
    IThemeSetterProxy themeSetterProxy,
    IThemeDialogProxy themeDialogProxy,
    IObservableStorage<ThemeSettings> themeSettingsStorage) : IThemeApplier
{
    private const string FallbackThemeName = "Primus";
    private const string ColorSuffix = "Color";

    // Имя ресурса выводится из свойства темы: WindowBackgroundColor → Background (+ Color/Brush в прокси).
    private static readonly PropertyInfo[] ThemeColorProperties =
        typeof(Theme).GetProperties()
            .Where(property => property.PropertyType == typeof(string))
            .ToArray();

    public async Task ApplySavedThemeAsync()
        => await ApplyThemeAsync(themeSettingsStorage.Current.CurrentTheme);

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

        logger.LogInformation("Применена тема {name}", name);
    }

    private async Task<Theme?> LoadThemeOrFallbackAsync(string name)
    {
        var theme = await themeCatalog.LoadThemeAsync(name);
        if (theme is not null)
            return theme;

        logger.LogWarning("Тема {name} не найдена, используется резервная тема {fallback}", name, FallbackThemeName);

        themeSettingsStorage.Current.CurrentTheme = FallbackThemeName;
        try
        {
            await themeDialogProxy.ShowThemeNotFoundDialogAsync(name);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Не удалось показать диалог «Тема не найдена»");
        }

        return await themeCatalog.LoadThemeAsync(FallbackThemeName);
    }
}
